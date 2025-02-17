import { useNavigate } from "react-router-dom";
import './dashboard.css';
import { useEffect } from "react";

const Dashboard = ({ user, setUser }) => {
    const navigate = useNavigate();

    const [productsInfo, setProductsInfo] = useState("");
    useEffect(()=>{
        fetch(`/products/${company_id}`)
        .then(response => response.json())
        .then(data => {setProductsInfo(data)})
    });
    let productsAmount = productsInfo.Length;


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
                            <button className="product-amount">Product Amount: {productsAmount}</button>
                        </form>
                        <form>
                            <button className="employees-amount">employees</button>
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
