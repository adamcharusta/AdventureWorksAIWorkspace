import { describe, expect, it } from 'vitest'

import { ApiError } from '@/api/customFetch'

import { getApiErrorMessage } from './api-error'

describe('getApiErrorMessage', () => {
  it('returns the fallback when the error is not an ApiError', () => {
    expect(getApiErrorMessage(new Error('Unknown'), 'Fallback')).toBe(
      'Fallback',
    )
  })

  it('returns the fallback when the ApiError body is missing', () => {
    expect(
      getApiErrorMessage(new ApiError(500, undefined, 'Failure'), 'Fallback'),
    ).toBe('Fallback')
  })

  it('prefers the first validation message from problem details', () => {
    const error = new ApiError(
      400,
      {
        errors: {
          title: ['', 'Title is required.'],
          message: ['Message is required.'],
        },
      },
      'Validation failed',
    )

    expect(getApiErrorMessage(error, 'Fallback')).toBe('Title is required.')
  })

  it('falls back to detail, then title, then the provided fallback', () => {
    expect(
      getApiErrorMessage(
        new ApiError(403, { detail: 'Access denied.', title: 'Forbidden' }, ''),
        'Fallback',
      ),
    ).toBe('Access denied.')
    expect(
      getApiErrorMessage(
        new ApiError(403, { detail: '   ', title: 'Forbidden' }, ''),
        'Fallback',
      ),
    ).toBe('Forbidden')
    expect(
      getApiErrorMessage(
        new ApiError(403, { detail: '   ', title: '   ' }, ''),
        'Fallback',
      ),
    ).toBe('Fallback')
  })
})
