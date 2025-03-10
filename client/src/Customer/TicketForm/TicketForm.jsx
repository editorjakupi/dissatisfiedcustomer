import { StrictMode, useEffect, useState } from 'react'
import { createRoot } from 'react-dom/client'
import { useParams } from 'react-router'
import '../../main.css'


export function TicketForm() {
  const params = useParams();
  
  const [ticketform, setTicketform] = useState([]);

  useEffect(() => {
    fetch("/api/ticketform?caseNumber=" + params.caseNr)
        .then((response) => response.json())
        .then((data) => {
          console.log("Fetched Ticket Message:", data)
          setTicketform(data);
        })
        .catch((error) => console.error("Error fetching ticket message:", error))
  }, []);

  return <main>
    <ul>
      <div><h1>{ticketform.title}</h1></div>
      <div><h3>Category</h3><select class="category" defaultValue={null}>
      <option key={null} value={null} hidden>Choose Category</option>
        {ticketform.categories?.map(category => 
          <option key={category.id} value={category.id}>{category.name}</option>)}
      </select></div>
      <div><h3>Product/Service</h3><select class="product" defaultValue={null}>
      <option key={null} value={null} hidden>Choose Product/Service</option>
        {ticketform.company_products?.map(product => 
          <option key={product.id} value={product.id}>{product.name}</option>)}
      </select></div>
      <div><h3>Message</h3><textarea class="message" placeholder='Write your problem in detail here' defaultValue={ticketform.description} /></div>
      <div><button class="cancel button">Cancel</button><button class="save button" onClick={() => saveTicketForm(ticketform.ticket_id)}>Save</button></div>
    </ul>
  </main>
}

function saveTicketForm(ticketId){
  console.log(JSON.stringify({
    "TicketId": ticketId,
    "ProductId": document.querySelector(".product").value,
    "CategoryId": document.querySelector(".category").value,
    "Content": document.querySelector(".message").value
  }));
  fetch("/api/ticketform", {
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    },
    method: "POST",
    body: JSON.stringify({
      "TicketId": ticketId,
      "ProductId": document.querySelector(".product").value,
      "CategoryId": document.querySelector(".category").value,
      "Content": document.querySelector(".message").value
    })
  })
    .then(response => response.json())  // Parsa JSON responsen från backenden
    .then(data => console.log(data));     // Logga responsen
}