import { useNavigate } from "react-router-dom";

const Dashboard = ({ user, setUser }) => {
    const navigate = useNavigate();

    const handleLogout = () => {
        setUser(null); // Clear user data
        navigate("/login");
    };

    return (
        <div>
            <h2>Welcome, {user?.name}!</h2>
            <p>Email: {user?.email}</p>
            <p>Phone: {user?.phoneNumber}</p>
            <p>Role ID: {user?.roleId}</p>
            <button onClick={handleLogout}>Logout</button>
        </div>
    );
};

export default Dashboard;
