import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route} from "react-router"
import React,{useState, useEffect} from 'react'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
    <Routes>
      <Route path="login" element={<login/>} />
    </Routes>
    </BrowserRouter>

  </StrictMode>,
)

function login(){

  const [user, getUser] = useState([])

  useEffect(() => {
    fetch("/api/login")
    .then((response) => response.json())
    .then((data => getUser(data)))
    .catch((error) => console.error("error fetching user:", error))
  },[] )
}