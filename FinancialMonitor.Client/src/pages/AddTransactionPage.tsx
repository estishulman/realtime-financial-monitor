import { useState, type FormEvent } from 'react'
import { createTransaction, runLoadTest } from '../services/transactionApi'

export function AddTransactionPage() {
  const [amount, setAmount] = useState('1500.50')
  const [currency, setCurrency] = useState('USD')
  const [message, setMessage] = useState('')
  const [isSending, setIsSending] = useState(false)
  const [isRunningLoadTest, setIsRunningLoadTest] = useState(false)
  const [loadTestMessage, setLoadTestMessage] = useState('')

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); setIsSending(true); setMessage('')
    try {
      await createTransaction({ transactionId: crypto.randomUUID(), amount: Number(amount), currency: currency.trim().toUpperCase(), status: 'Pending', timestamp: new Date().toISOString() })
      setMessage('Transaction accepted and queued for live delivery.')
    } catch { setMessage('Could not reach the API. Check that the backend is running.') }
    finally { setIsSending(false) }
  }

  async function handleLoadTest() {
    setIsRunningLoadTest(true)
    setLoadTestMessage('')

    try {
      const result = await runLoadTest()
      setLoadTestMessage(`${result.accepted} accepted, ${result.rejected} rejected.`)
    } catch {
      setLoadTestMessage('Load test could not reach the API.')
    } finally {
      setIsRunningLoadTest(false)
    }
  }

  return <section className="page-grid single-column">
    <div className="page-heading"><p className="eyebrow">INGESTION / TEST FEED</p><h2>Send a transaction</h2><p>Create a realistic event and watch it arrive on the live monitor.</p></div>
    <form className="transaction-form" onSubmit={handleSubmit}>
      <label>Amount<input type="number" min="0" step="0.01" value={amount} onChange={(event) => setAmount(event.target.value)} required /></label>
      <label>Currency<input value={currency} maxLength={3} onChange={(event) => setCurrency(event.target.value)} required /></label>
      <button className="primary-button" disabled={isSending} type="submit">{isSending ? 'Sending...' : 'Send transaction'}</button>
      {message && <p className="form-message" role="status">{message}</p>}
    </form>
    <section className="load-test-panel" aria-labelledby="load-test-title">
      <div>
        <p className="eyebrow">PERFORMANCE CHECK</p>
        <h3 id="load-test-title">Transaction load test</h3>
        <p>Send 100 transactions to verify the live pipeline under burst traffic.</p>
      </div>
      <button className="secondary-button" disabled={isRunningLoadTest} onClick={() => void handleLoadTest()} type="button">
        {isRunningLoadTest ? 'Running load test...' : 'Run 100-Transaction Load Test'}
      </button>
      {loadTestMessage && <p className="form-message" role="status">{loadTestMessage}</p>}
    </section>
  </section>
}