import { NavLink, Outlet } from 'react-router-dom'
import { TransactionRealtimeProvider } from '../contexts/TransactionRealtimeProvider'

export function AppShell() {
  return <TransactionRealtimeProvider><div className="app-shell">
    <header className="topbar">
      <div className="brand-mark">FM</div>
      <div><p className="eyebrow">OPERATIONS CONSOLE</p><h1>Financial Monitor</h1></div>
      <nav className="nav-links" aria-label="Main navigation">
        <NavLink to="/monitor">Live monitor</NavLink>
        <NavLink to="/add">Add transaction</NavLink>
      </nav>
    </header>
    <main className="page-content"><Outlet /></main>
  </div></TransactionRealtimeProvider>
}