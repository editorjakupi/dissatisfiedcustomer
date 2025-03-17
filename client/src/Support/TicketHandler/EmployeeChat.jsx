import React, { useState } from "react";
import "../../main.css";

// Komponent för att låta anställda skicka meddelanden till ett kundärende.
const EmployeeChat = ({ ticket, fetchMessages }) => {
  const [messageContent, setMessageContent] = useState(""); // Håller innehållet i meddelandefältet.

  // Hanterar när formuläret skickas för att lägga till ett nytt meddelande.
  const handleSubmit = (e) => {
    e.preventDefault(); // Förhindrar att sidan laddas om.

    // Kontrollerar om ärendet är stängt eller löst och visar en varning om inga nya meddelanden kan läggas till.
    if (ticket.status.toLowerCase() === "closed" || ticket.status.toLowerCase() === "resolved") {
      const alertMessage =
        ticket.status.toLowerCase() === "resolved"
          ? "Ticket is resolved. You cannot add new messages."
          : "Ticket is closed. You cannot add new messages.";
      alert(alertMessage); // Visar varning till användaren.
      return; // Avslutar funktionen om ärendet inte kan uppdateras.
    }

    // Skickar en POST-förfrågan till servern för att lägga till ett nytt meddelande.
    //app.MapPost api endpoint definierat i program.cs, och metoden för databasoperationen finns i CaseRoutes, AddCaseMessageByEmail
    const url = `http://localhost:5000/api/tickets/handle/${ticket.id}/messages`; // Bygger URL för API-anropet.
    console.log("POST URL (Employee):", url);
    fetch(url, {
      method: "POST", // Anger HTTP-metoden som POST.
      headers: {
        "Content-Type": "application/json" // Anger att JSON används som datatyp.
      },
      body: JSON.stringify({ content: messageContent }), // Skickar meddelandeinnehållet som JSON.
      credentials: "include" // Inkluderar autentisering och sessionscookies.
    })
      .then(response => {
        if (response.status === 204) {
          return {}; // Hanterar tomt svar från servern.
        }
        return response.json().catch(() => ({})); // Försöker tolka JSON-svar även om det är tomt.
      })
      .then(() => {
        setMessageContent(""); // Rensar meddelandefältet efter lyckad skickning.
        fetchMessages(ticket.id); // Uppdaterar meddelandelistan efter att ett meddelande har lagts till.
      })
      .catch(error => {
        alert(error.message); // Visar felmeddelande för användaren om API-anropet misslyckas.
        console.error("Error adding message (Employee):", error); // Loggar felet i konsolen.
      });
  };

  // Renderar formuläret för att lägga till ett meddelande.
  return (
    <form className="employee-chat-form" onSubmit={handleSubmit}>
      <textarea
        value={messageContent} // Kopplar textfältets värde till komponentens state.
        onChange={(e) => setMessageContent(e.target.value)} // Uppdaterar state när användaren skriver i textfältet.
        placeholder="Enter your message" // Placeholder-text i textfältet.
        required // Gör att textfältet måste fyllas i innan formuläret kan skickas.
      />
      <div className="button-container">
        <button type="submit" className="add-message-button">
          Add Message
        </button>
      </div>
    </form>
  );
};

export default EmployeeChat;
