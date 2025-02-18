import { useState } from "react";
import { useNavigate } from "react-router";

const Login = ({ user, setUser }) => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");

        try {
            const response = await fetch("api/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password })
            });

            if (!response.ok) throw new Error("Invalid email or password");

            const data = await response.json();

            // Convert roleId to role_id
            const userWithRoleIdFixed = { ...data, role_id: data.roleId, roleId: undefined };

            setUser(userWithRoleIdFixed);
            localStorage.setItem("user", JSON.stringify(userWithRoleIdFixed)); // Save corrected user object
            navigate("/dashboard");
        } catch (err) {
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