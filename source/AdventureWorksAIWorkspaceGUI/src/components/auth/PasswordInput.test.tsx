import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import { PasswordInput } from './PasswordInput'

describe('<PasswordInput />', () => {
  it('renders password field by default', () => {
    render(
      <PasswordInput
        value="secret"
        onChange={() => undefined}
        required
        fullWidth
        handleClickShowPassword={() => undefined}
      />,
    )

    const input = screen.getByLabelText(/password/i, { selector: 'input' })
    expect(input).toHaveAttribute('type', 'password')
  })

  it('renders plain text type when showPassword is true', () => {
    render(
      <PasswordInput
        value="secret"
        onChange={() => undefined}
        required
        fullWidth
        showPassword
        handleClickShowPassword={() => undefined}
      />,
    )

    const input = screen.getByLabelText(/password/i, { selector: 'input' })
    expect(input).toHaveAttribute('type', 'text')
  })

  it('calls toggle handler when visibility icon is clicked', async () => {
    const user = userEvent.setup()
    const handleClickShowPassword = vi.fn()

    render(
      <PasswordInput
        value="secret"
        onChange={() => undefined}
        required
        fullWidth
        handleClickShowPassword={handleClickShowPassword}
      />,
    )

    await user.click(
      screen.getByRole('button', { name: /display the password/i }),
    )

    expect(handleClickShowPassword).toHaveBeenCalledTimes(1)
  })
})
