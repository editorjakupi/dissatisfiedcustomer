// src/SessionTest.jsx
import React, { useEffect, useState } from 'react';

// En komponent för att testa och visa sessionsdata från servern.
const SessionTest = () => {
  const [sessionData, setSessionData] = useState(null); // Håller sessionsdata som hämtas från servern.

  // Effekt för att hämta sessionsdata vid montering av komponenten.
  useEffect(() => {
    fetch("http://localhost:5000/api/session", {
      method: "GET", // Anger HTTP-metoden för att hämta data.
      credentials: "include" // Viktigt: Skickar med session-cookien för autentisering.
    })
      .then(response => {
        if (!response.ok) { // Kontrollerar om svaret inte är framgångsrikt.
          throw new Error("Network response was not ok"); // Kastar ett fel om något är fel.
        }
        return response.json(); // Tolkar svar till JSON-format.
      })
      .then(data => {
        console.log("Session data:", data); // Loggar den hämtade sessionsdatan för debugging.
        setSessionData(data); // Sparar den hämtade data i state.
      })
      .catch(error => console.error("Error:", error)); // Loggar eventuella fel.
  }, []); // Kör bara en gång vid komponentens montering.

  // Renderar innehåll beroende på om sessionsdata har hämtats.
  return (
    <div>
      <h2>Session Test</h2>
      {sessionData ? ( // Kontrollerar om sessionsdata finns.
        <pre>{JSON.stringify(sessionData, null, 2)}</pre> // Visar sessionsdata i ett snyggt JSON-format.
      ) : (
        <p>No session data found.</p> // Visar meddelande om ingen data hittas.
      )}
    </div>
  );
};

export default SessionTest;
