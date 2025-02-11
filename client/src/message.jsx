import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './message.css'

export function Message() {
  return <main>
    <ul>
      <div><h3>Title</h3><input /></div>
      <div><h3>Email</h3><input /></div>
      <div><h3>Message</h3><input /></div>
      <div><button class="cancel button">Cancel</button><button class="submit button">Submit</button></div>
    </ul>
  </main>
}