import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import { renderWithProviders } from '@/test/render-utils'

import { ChatDrawer } from './ChatDrawer'
import { MenuDrawer } from './MenuDrawer'

const drawerItems = [
  { label: 'Workspace', icon: <DashboardRoundedIcon />, selected: true },
]

describe('<MenuDrawer />', () => {
  it('renders collapse control when drawer is open', () => {
    renderWithProviders(
      <MenuDrawer
        items={drawerItems}
        open
        onToggle={() => undefined}
        title="Main menu"
        subtitle="Reports"
      />,
    )

    expect(
      screen.getByRole('button', { name: /collapse menu/i }),
    ).toBeInTheDocument()
  })

  it('renders expand control when drawer is collapsed', () => {
    renderWithProviders(
      <MenuDrawer
        items={drawerItems}
        open={false}
        onToggle={() => undefined}
        title="Main menu"
        subtitle="Reports"
      />,
    )

    expect(
      screen.getByRole('button', { name: /expand menu/i }),
    ).toBeInTheDocument()
  })

  it('calls onToggle and bottom item actions', async () => {
    const user = userEvent.setup()
    const onToggle = vi.fn()
    const onLogout = vi.fn()

    renderWithProviders(
      <MenuDrawer
        bottomItems={[
          {
            label: 'Log out',
            icon: <LogoutRoundedIcon />,
            onClick: onLogout,
          },
        ]}
        items={drawerItems}
        open
        onToggle={onToggle}
        title="Main menu"
        subtitle="Reports"
      />,
    )

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

    await user.click(
      screen.getByRole('button', { name: /expand chat drawer/i }),
    )

    expect(onToggle).toHaveBeenCalledTimes(1)
  })
})
