import { useState } from "react";
import { useNavigate } from "react-router";

const Login = ({ setUser }) => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");

        try {
            console.log("Sending request with:", {email, password});
            
            const response = await fetch("api/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({email, password})
            });


            console.log("Request Status:", response.status);
            const data = await response.json();
            console.log("Response Data:", data);


            if (!response.ok) {
                throw new Error("Invalid email or password")
            };
            
            setUser(data);
            navigate("/dashboard");
        } catch (err) {
            console.error("Error during login:", err)
            setError("Invalid email or password.");
        }
    };

    return (
        <div>
            <h2>Login</h2>
            {error && <p style={{ color: "red" }}>{error}</p>}
            <form onSubmit={handleSubmit}>
                <div>
                    <label>Email:</label>
                    <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
                </div>
                <div>
                    <label>Password:</label>
                    <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
                </div>
                <button type="submit">Login</button>
            </form>
        </div>
    );
};

export default Login;
