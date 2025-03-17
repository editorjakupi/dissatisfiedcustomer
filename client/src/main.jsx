import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom/client";
// Vi använder BrowserRouter från react-router-dom om vi driver en webbaserad app
import { BrowserRouter as Router, Routes, Route } from "react-router"; 
import Login from "./User/Login/Login.jsx";
import Dashboard from "./User/Dashboard/dashboard.jsx";
import AccountInformation from "./User/Account/account.jsx";
import PasswordForget from "./User/passwordforget.jsx";
import NavBar from "./User/NavBar/NavBar.jsx";
import TicketView from "./Support/TicketView/TicketView.jsx";
import UsersList from "./Admin/UsersList/UsersList.jsx";
import { TicketForm } from "./Customer/TicketForm/TicketForm.jsx";
import NewEmployee from "./Admin/New/NewEmployee.jsx";
import AdminList from "./SuperAdmin/AdminList.jsx";
import { DemoPage } from "./Customer/DemoPages/DemoPage.jsx";

import "./main.css";
import { Message } from "./Customer/Message/message.jsx";
import TicketHandler from "./Support/TicketHandler/TicketHandler.jsx";
import NewProduct from "./Admin/New/NewProduct.jsx";
import NewCompany from "./SuperAdmin/NewCompany.jsx";
import CustomerCases from "./Customer/CustomerCases/CustomerCases.jsx";
import SessionTest from './SessionTest'; // Importera komponenten


// Importera eventuella CSS-filer
import FeedbackView from "./Admin/Feedback/FeedbackView.jsx";

// Huvudapplikationen som hanterar routing och användarsession.
const App = () => {
    const [user, setUser] = useState(null); // Håller information om inloggad användare.

    // Hämtar sessionens användarinformation när applikationen startar.
    useEffect(() => {
        const fetchSessionUser = async () => {
            try {
                const response = await fetch("/api/session", { method: "GET", headers: { "Content-Type": "application/json" } });
                if (!response.ok) {
                    setUser(null); // Om svaret inte är okej, sätt användare till null.
                    return;
                }
                const text = await response.text();
                if (!text) {
                    setUser(null); // Om ingen användardata returneras, sätt till null.
                    return;
                }
                const userData = JSON.parse(text); // Parsar användardata från JSON.
                setUser(userData || null); // Sparar användardata i state.
            } catch (error) {
                console.error("Error fetching session:", error); // Loggar fel om sessionen inte kan hämtas.
                setUser(null); // Sätter användare till null vid fel.
            }
        };

        fetchSessionUser(); // Anropar funktionen för att hämta sessionen.
    }, []);

    // Returnerar applikationens innehåll och router-konfiguration.
    return (
        <Router>
            <div className="app-container">
                {/* NavBar visas bara för inloggade anställda/admin/superadmin */}
                {user && <NavBar user={user} setUser={setUser} />}
                <div className="content">
                    <Routes>
                        {/* Inloggningssida: Endast employees, admin och superadmin loggar in */}
                        <Route path="/" element={<Login setUser={setUser} />} />
                        <Route path="/dashboard" element={user ? <Dashboard user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/tickets" element={user ? <TicketView user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/account" element={user ? <AccountInformation user={user} setUser={setUser} /> : <Login setUser={setUser} />} />
                        <Route path="/forgot-password" element={<PasswordForget />} />
                        <Route path="/ticketform/:caseNr" element={<TicketForm />} />
                        <Route path="/users" element={user ? <UsersList user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/employee" element={user ? <NewEmployee user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/products" element={user ? <NewProduct user={user} /> : <Login setUser={setUser} />} />
                        <Route path="/feedback" element={user ? <FeedbackView user={user} /> : <Login setUser={setUser} />} />
                        
                        {/* Employee-endpoint för att hantera ärenden */}
                        <Route path="/tickets/handle/:ticketId" element={<TicketHandler />} />

                        {/* Kundens chattvy: Kunden får en bekräftelselänk med token (ingen inloggning) */}
                        <Route path="/tickets/view/:token" element={<CustomerCases />} />

                        {/* Endast en inloggad kund har inte längre ett konto, så vi tar bort /message/:id från kundens vy */}
                        <Route path="/message/:id" element={<Message />} />
                        <Route path="/demopage/:id" element={<DemoPage />} />
                        <Route path="/session-test" element={<SessionTest />} />
                        <Route path="/admins" element={user ? <AdminList user={user} /> : <Login setUser={user}/>}/>
                        <Route path="/companies" element={user ? <NewCompany user={user}/> : <Login setUser={user}/>} />
                    </Routes>
                </div>
            </div>
        </Router>
    );
};

// Startar applikationen genom att rendera den i root-elementet.
ReactDOM.createRoot(document.getElementById("root")).render(
    <React.StrictMode>
        <App />
    </React.StrictMode>
);
