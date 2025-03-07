// src/SessionTest.jsx
import React, { useEffect, useState } from 'react';

const SessionTest = () => {
  const [sessionData, setSessionData] = useState(null);

  useEffect(() => {
    fetch("http://localhost:5000/api/session", {
      method: "GET",
      credentials: "include" // Viktigt: Skickar med cookien (session)
    })
      .then(response => {
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        return response.json();
      })
      .then(data => {
        console.log("Session data:", data);
        setSessionData(data);
      })
      .catch(error => console.error("Error:", error));
  }, []);

  return (
    <div>
      <h2>Session Test</h2>
      {sessionData ? (
        <pre>{JSON.stringify(sessionData, null, 2)}</pre>
      ) : (
        <p>No session data found.</p>
      )}
    </div>
  );
};

export default SessionTest;
