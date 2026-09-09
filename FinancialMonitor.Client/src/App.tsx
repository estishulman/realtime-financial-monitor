import { Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from './components/AppShell'
import { AddTransactionPage } from './pages/AddTransactionPage'
import { MonitorPage } from './pages/MonitorPage'
import './App.css'

export default function App() {
  return <Routes><Route element={<AppShell />}><Route index element={<Navigate to="/monitor" replace />} /><Route path="/add" element={<AddTransactionPage />} /><Route path="/monitor" element={<MonitorPage />} /></Route></Routes>
}
