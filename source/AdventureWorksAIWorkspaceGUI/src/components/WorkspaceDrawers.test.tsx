import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import { renderWithProviders } from '../test/render-utils'
import { ChatDrawer } from './ChatDrawer'
import { MenuDrawer } from './MenuDrawer'

describe('<MenuDrawer />', () => {
  it('renders collapse control when drawer is open', () => {
    renderWithProviders(
      <MenuDrawer open onToggle={() => undefined} onLogout={() => undefined} />,
    )

    expect(
      screen.getByRole('button', { name: /collapse menu/i }),
    ).toBeInTheDocument()
  })

  it('renders expand control when drawer is collapsed', () => {
    renderWithProviders(
      <MenuDrawer
        open={false}
        onToggle={() => undefined}
        onLogout={() => undefined}
      />,
    )

    expect(screen.getByRole('button', { name: /expand menu/i })).toBeInTheDocument()
  })

  it('calls onToggle and onLogout actions', async () => {
    const user = userEvent.setup()
    const onToggle = vi.fn()
    const onLogout = vi.fn()

    renderWithProviders(<MenuDrawer open onToggle={onToggle} onLogout={onLogout} />)

    await user.click(screen.getByRole('button', { name: /collapse menu/i }))
    await user.click(screen.getByRole('button', { name: /log out/i }))

    expect(onToggle).toHaveBeenCalledTimes(1)
    expect(onLogout).toHaveBeenCalledTimes(1)
  })
})

describe('<ChatDrawer />', () => {
  it('renders as a right-anchored drawer and shows collapse control when open', () => {
    const { container } = renderWithProviders(
      <ChatDrawer open onToggle={() => undefined} />,
    )

    expect(container.querySelector('.MuiDrawer-root')).toHaveClass(
      'MuiDrawer-anchorRight',
    )
    expect(
      screen.getByRole('button', { name: /collapse chat drawer/i }),
    ).toBeInTheDocument()
  })

  it('renders expand control and calls onToggle when collapsed', async () => {
    const user = userEvent.setup()
    const onToggle = vi.fn()

    renderWithProviders(<ChatDrawer open={false} onToggle={onToggle} />)

    await user.click(screen.getByRole('button', { name: /expand chat drawer/i }))

    expect(onToggle).toHaveBeenCalledTimes(1)
  })
})