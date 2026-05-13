import { ApiException } from '../api/form-builder-api';

export interface ApiErrorResult {
  message: string;
  errors?: string[];
}

export function extractApiError(err: any): ApiErrorResult {
  // NSwag throws the parsed result object directly when the response body is valid JSON
  if (err && typeof err === 'object' && !ApiException.isApiException(err) && typeof err.message === 'string') {
    return {
      message: err.message,
      errors: Array.isArray(err.errors) ? err.errors : undefined
    };
  }

  // NSwag ApiException carries the raw response string
  if (ApiException.isApiException(err)) {
    try {
      const parsed = JSON.parse(err.response);

      // Our middleware format: { message, errors? }
      // ASP.NET Core ValidationProblemDetails: { title, errors: { "Field": ["msg"] } }
      const message: string = parsed?.message ?? parsed?.title;
      if (message) {
        let errors: string[] | undefined;

        if (Array.isArray(parsed.errors)) {
          errors = parsed.errors;
        } else if (parsed.errors && typeof parsed.errors === 'object') {
          // Flatten { "Label": ["msg1"], "Options[0]": ["msg2"] } → ["msg1", "msg2"]
          errors = (Object.values(parsed.errors) as string[][]).flat();
        }

        return { message, errors };
      }
    } catch { /* not JSON — fall through */ }

    return statusCodeMessage(err.status);
  }

  // Network / CORS / offline
  if (err?.status === 0) {
    return { message: 'Cannot reach the server. Please check your internet connection.' };
  }

  if (err?.status) {
    return statusCodeMessage(err.status);
  }

  return { message: 'An unexpected error occurred. Please try again.' };
}

function statusCodeMessage(status: number): ApiErrorResult {
  switch (status) {
    case 400: return { message: 'The request was invalid. Please check your input.' };
    case 401: return { message: 'Your session has expired. Please sign in again.' };
    case 403: return { message: 'You do not have permission to perform this action.' };
    case 404: return { message: 'The requested resource was not found.' };
    case 409: return { message: 'This conflicts with existing data. Please try a different value.' };
    case 422: return { message: 'Validation failed. Please check your input and try again.' };
    case 500: return { message: 'A server error occurred. Please try again later.' };
    default:  return { message: 'An unexpected error occurred. Please try again.' };
  }
}
