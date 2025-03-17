import React, { useState, useEffect } from "react";
import "../../main.css";

// Komponent som låter användaren skicka nya meddelanden till ett ärende.
const AddMessageForm = ({ token, userEmail, onMessageAdded, isSessionActive, ticketStatus }) => {
  const [messageContent, setMessageContent] = useState(""); // Håller innehållet i meddelandefältet.

  // Loggar ärendets status när det ändras (för debugging).
  useEffect(() => {
    console.log("Ticket Status in Component:", ticketStatus);
  }, [ticketStatus]);

  // Hanterar när formuläret skickas.
  const handleSubmit = (e) => {
    e.preventDefault(); // Förhindrar sidans omladdning vid skickning.
    
    // Kontrollerar om sessionen är aktiv. Om inte, visa en varning baserat på ärendets status.
    if (!isSessionActive) {
      const alertMessage =
        ticketStatus === "Resolved"
          ? "Ticket is resolved. You cannot add new messages."
          : "Ticket is closed. You cannot add new messages.";
      alert(alertMessage); // Visar varning och avslutar funktionen.
      return;
    }

    // Skickar POST-begäran för att lägga till ett nytt meddelande.
    const url = `http://localhost:5000/api/tickets/view/${token}/messages`; // URL till serverns endpoint.
    console.log("POST URL (Customer):", url);
    // Skickar POST-begäran till servern. app.MapPost endpointen är definierat i program.cs, metoden för databasoperationen finns i CaseRoutes, AddCaseMessageByEmail
    fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json" // Anger JSON som format.
      },
      body: JSON.stringify({ email: userEmail, content: messageContent }), // Innehållet som skickas.
      credentials: "include" // Inkluderar autentiseringsdata.
    })
      .then(response => {
        if (response.status === 204) {
          return {}; // Hanterar tomt svar från servern.
        }
        return response.json().catch(() => ({})); // Försöker tolka JSON-svar även om det är tomt.
      })
      .then(() => {
        setMessageContent(""); // Rensar textfältet efter lyckad skickning.
        onMessageAdded(); // Triggar callback för att uppdatera meddelandelistan.
      })
      .catch(error => {
        alert(error.message); // Visar felmeddelande vid misslyckad skickning.
        console.error("Error adding message (Customer):", error); // Loggar felet i konsolen.
      });
  };

  // Renderar formuläret för att skicka ett nytt meddelande.
  return (
    <form className="add-message-form" onSubmit={handleSubmit}>
      <textarea
        value={messageContent} // Kopplar textfältet till state.
        onChange={(e) => setMessageContent(e.target.value)} // Uppdaterar state när användaren skriver.
        placeholder="Enter your message" // Placeholder-text i fältet.
        required // Kräver att fältet fylls i.
      />
      <div className="button-container">
        <button type="submit" className="add-message-button" disabled={!isSessionActive}>
          Add Message
        </button>
      </div>
    </form>
  );
};

export default AddMessageForm;
