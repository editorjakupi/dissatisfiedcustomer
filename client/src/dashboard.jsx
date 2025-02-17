import { useNavigate } from "react-router-dom";
import './dashboard.css';

const Dashboard = ({ user, setUser }) => {
    const navigate = useNavigate();

    const handleLogout = () => {
        setUser(null); // Clear user data
        navigate("/login");
    };

    return (
        <main id="dashboard-main">
            {user.roleId === 1 && (
                <p>customer</p>
                // Link to customer page
            )}
            {user.roleId === 2 && (
                <p>employee</p>
            )}
            {user.roleId === 3 && (
                <div className="admin-dashboard-div">
                    <div className="admin-dashboard-content-div">
                        <form>
                            <button>products</button>
                        </form>
                        <form>
                            <button>employees</button>
                        </form>
                    </div>
                </div>
            )}
            {user.roleId === 4 && (
                <p>superadmin</p>
                // Super admin dashboard
            )}
        </main>
    );
};


export default Dashboard;
