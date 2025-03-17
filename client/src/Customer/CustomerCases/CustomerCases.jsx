import React, { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router';
import AddMessageForm from '../Message/AddMessageForm';
import SessionTimer from '../SessionTest&Timer/SessionTimer';
import '../../main.css';

// Komponent för att hantera kundärenden och meddelanden i en chatt-session.
const CustomerCases = () => {
  const { token } = useParams(); // Hämtar token från URL:en för att identifiera ärendet.
  const [ticket, setTicket] = useState(null); // Håller information om det aktuella ärendet.
  const [messages, setMessages] = useState([]); // Håller meddelanden kopplade till ärendet.
  const [error, setError] = useState(null); // Håller eventuella fel.
  const [isSessionActive, setIsSessionActive] = useState(true); // Spårar om sessionen är aktiv.
  const [sessionExpiry, setSessionExpiry] = useState(3600); // Sessionens längd i sekunder.
  const messagesEndRef = useRef(null); // Referens för att automatiskt scrolla till det senaste meddelandet.

  // Hämtar ärendedata och startar polling för nya meddelanden.
  useEffect(() => {
    if (!token) return; // Om ingen token finns, avbryt.
    //fetchar från api rutt app.MapGet endpointen som definieras i program.cs och själva databasoperationen för att hämta biljetten implementeras i TicketRoutes.cs GetTicketByToken
    fetch(`http://localhost:5000/api/tickets/view/${token}`, {
      method: 'GET',
      credentials: 'include', // Inkluderar sessionsinformation.
      headers: { 'Cache-Control': 'no-cache' } // Inaktiverar cache för att säkerställa uppdaterad data.
    })
      .then(response => {
        if (!response.ok) return response.text().then(text => { throw new Error(text || 'Ticket not found'); });
        return response.json(); // Returnerar ärendedata i JSON-format.
      })
      .then(data => {
        console.log("Fetched ticket:", data);
        setTicket(data); // Sparar ärendedata i state.
        const status = data.status ? data.status.toLowerCase().trim() : ""; // Hämtar ärendestatus.
        setIsSessionActive(status !== "closed" && status !== "resolved"); // Avaktiverar sessionen om ärendet är stängt eller löst.
        if (data && data.id) {
          fetchMessages(data.id); // Hämtar meddelanden kopplade till ärendet.
          startPolling(data.id); // Startar polling för kontinuerliga uppdateringar.
        } else {
          throw new Error("Ticket data is incomplete."); // Fel om ärendedata saknas.
        }
      })
      .catch(err => {
        console.error("Error fetching ticket:", err);
        setError(err.message); // Sparar felet i state.
      });
  }, [token]);

  // Funktion som startar polling för att hämta nya meddelanden kontinuerligt.
  const startPolling = (ticketId) => {
    const intervalId = setInterval(() => {
      console.log("Polling messages for ticket:", ticketId);
      fetchMessages(ticketId); // Hämtar meddelanden för ärendet.
    }, 5000); // Polling var 5:e sekund.
    return () => clearInterval(intervalId); // Stoppar polling när komponenten avmonteras.
  };

  // Scrollar automatiskt till det senaste meddelandet när meddelandelistan uppdateras.
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  // Hämtar alla meddelanden kopplade till ett specifikt ärende-ID.
  const fetchMessages = (ticketId) => {
    console.log("Fetching messages for ticket:", ticketId);
    //fetchar från api GET endpoint som definieras i program.cs och databasoperationen implementeras i CaseRoutes GetCaseMessagesByEmail
    fetch(`/api/tickets/${ticketId}/messages`, {
      method: "GET",
      credentials: "include", // Inkluderar sessionscookies.
      headers: { "Cache-Control": "no-cache" } // Förhindrar cache av data.
    })
      .then(response => {
        if (!response.ok) return response.text().then(text => { throw new Error(text || "Error fetching messages"); });
        return response.json(); // Returnerar meddelandedata i JSON-format.
      })
      .then(data => {
        console.log("Fetched messages (raw):", data);
        setMessages(data); // Uppdaterar state med hämtade meddelanden.
      })
      .catch(err => {
        console.error("Error fetching messages:", err);
        setError(err.message); // Sparar fel om hämtning misslyckas.
      });
  };

  // Uppdaterar meddelandelistan när ett nytt meddelande läggs till.
  const handleMessageAdded = () => {
    if (ticket && ticket.id) {
      fetchMessages(ticket.id); // Hämtar den uppdaterade meddelandelistan.
    }
  };

  // Returnerar felmeddelande om något går fel.
  if (error) {
    return <p className="error">Error: {error}</p>;
  }

  // Visar laddningsmeddelande tills ärendet är hämtat.
  if (!ticket) {
    return <p>Loading chat session...</p>;
  }

  // Renderar ärendedetaljer, meddelanden och formulär för att lägga till nya meddelanden.
  return (
    <div className="cases-container">
      <h2>Chat Session</h2>
      {isSessionActive && <SessionTimer durationInSeconds={sessionExpiry} />} {/* Timer visas om sessionen är aktiv */}
      <div className="case-details">
        <p><strong>Case Number:</strong> {ticket.caseNumber}</p>
        <p><strong>Title:</strong> {ticket.title}</p>
        <p><strong>Description:</strong> {ticket.description}</p>
        <p><strong>Status:</strong> {ticket.status}</p>
        <p><strong>Date:</strong> {new Date(ticket.date).toLocaleDateString()}</p>
      </div>
      <h4>Messages</h4>
      <ul className="messages-list">
        {messages && messages.length > 0 ? ( // Kontrollerar om det finns meddelanden att visa.
          messages.map((msg) => {
            const senderType = msg.senderType?.trim().toLowerCase() || ""; // Identifierar avsändartyp.
            const isEmployeeMessage = senderType === "employee"; // Kontrollera om meddelandet är från kundtjänst.
            const prefix = isEmployeeMessage ? "Customer Service: " : "Customer: "; // Prefix för avsändare.
            const messageClass = isEmployeeMessage ? "employee-message" : "customer-message"; // CSS-klass för meddelande.
            return (
              <li key={msg.messageId} className={`message-item ${messageClass}`}>
                <p>{prefix}{msg.content}</p>
              </li>
            );
          })
        ) : (
          <li className="message-item">No messages yet.</li> // Visar meddelande om inga meddelanden finns.
        )}
        <div ref={messagesEndRef}></div> {/* Referens för att scrolla till det senaste meddelandet */}
      </ul>
      <h4>Add a Message</h4>
      <AddMessageForm 
        token={token} // Skickar token för att identifiera aktuell session.
        userEmail={ticket.email} // Kundens e-post kopplad till ärendet.
        onMessageAdded={handleMessageAdded} // Callback som triggas när ett meddelande läggs till.
        isSessionActive={isSessionActive} // Kontrollerar om sessionen är aktiv.
        ticketStatus={ticket.status} // Skickar ärendets status.
      />
    </div>
  );
};

export default CustomerCases;
