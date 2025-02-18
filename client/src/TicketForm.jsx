import { StrictMode, useEffect, useState } from 'react'
import { createRoot } from 'react-dom/client'
import { useParams } from 'react-router'
import './TicketForm.css'

export function TicketForm() {
  const params = useParams();
  const [ticketform, setTicketform] = useState([]);


  useEffect(() => {
    fetch("/api/ticketform/" + params.caseNr)
      .then((response) => response.json())
      .then((data) => {
        console.log("Fetched Ticket Message:", data)
        setTicketform(data);
      })
      .catch((error) => console.error("Error fetching ticket message:", error))
  }, []);

  return <main>
    <ul>
      <div><h3>Title</h3><input class="title" placeholder={ticketform.title} /></div>
      <div><h3>Email</h3><select class="product select">
        <option>LMAO</option>
        <option>XD</option>
      </select></div>
      <div><h3>Message</h3><textarea class="message" placeholder='Write your problem in detail here' /></div>
      <div><button class="cancel button">Cancel</button><button class="save button" onClick="">Save</button></div>
    </ul>
  </main>
}