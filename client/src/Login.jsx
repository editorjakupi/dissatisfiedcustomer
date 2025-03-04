import React, { useState, useEffect } from "react";
import { useNavigate, Link } from "react-router";
import "./NavBar.css";

const Login = ({ user, setUser }) => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");

        try {
            const response = await fetch("/api/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "include", // Ensures session cookie is saved
                body: JSON.stringify({ email, password }),
            });

            if (!response.ok) throw new Error("Invalid email or password");

            const data = await response.json();
            setUser(data);
            navigate("/dashboard");
        } catch (err) {
            setError("Invalid email or password.");
        }
    };


    useEffect(() => {
        const fetchSessionUser = async () => {
            try {
                const response = await fetch("/api/session", {
                    method: "GET",
                    headers: { "Content-Type": "application/json" }
                });

                if (!response.ok) {
                    console.error("Session fetch failed");
                    setUser(null); // Ensure user is null if not logged in
                    return;
                }

                // Check if response body is empty before parsing JSON
                const text = await response.text();
                if (!text) {
                    setUser(null); // If empty, treat as no user
                    return;
                }

                const userData = JSON.parse(text);
                setUser(userData || null); // Ensure user is null if no session exists
            } catch (error) {
                console.error("Error fetching session:", error);
                setUser(null); // Fallback to null if fetch fails
            }
        };


        fetchSessionUser();
    }, []);


    return (
        <main>
        <div id="input-login-div">
            {error && <p style={{color: "red"}}>{error}</p>}
            <form onSubmit={handleSubmit}>

                <div id="input-div">
                    <div id="login-input-button-div">
                        <label>
                            <p>Email-adress:</p>
                            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required/>
                        </label>
                    </div>

                    <div id="login-input-button-div">
                        <label>
                            <p>Password:</p>
                            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)}
                                   required/>
                        </label>
                    </div>
                    <div className="forgot-password-div">
                        Forgot-password? &nbsp;
                        <button id="button" onClick={() => navigate("/forgot-password")}>Click Me!</button>
                    </div>
                    
                    <div id="update-button-div">
                        <button type="submit">Login</button>
                    </div>
                </div>
            </form>
        </div>
        </main>
    );
};

export default Login;