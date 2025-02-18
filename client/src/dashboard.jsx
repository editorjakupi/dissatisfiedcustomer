import { useNavigate } from "react-router-dom";
import './dashboard.css';
import { useEffect, useState } from "react";

const Dashboard = ({ user, setUser }) => {
    const navigate = useNavigate();

    const [employeesInfo, setEmployeesInfo] = useState([]);
    useEffect(()=>{
        fetch(`/api/employees/${user?.role_id}`)
        .then(response => response.json())
        .then(data => {setEmployeesInfo(data)})
    });

    /* 
    const [productsInfo, setProductsInfo] = useState([]);

    // Will be changed when session is saved on server side instead of client
    useEffect(()=>{
        fetch(`/api/products/11`) 
        .then(response => response.json())
        .then(data => {setProductsInfo(data)})
    }); 
    */


    const handleLogout = () => {
        setUser(null); // Clear user data
        navigate("/login");
    };

    return (
        <main id="dashboard-main">
            {user.role_id === 1 && (
                <p>customer</p>
                // Link to customer page
            )}
            {user.role_id === 2 && (
                <p>employee</p>
            )}
            {user?.role_id === 3 && (
                <div className="admin-dashboard-div">
                    <div className="admin-dashboard-content-div">
                        <form>
                            <button className="product-stats" onClick={() => navigate("/products")}>Product Amount: {/*{productsInfo.length}*/}</button>
                        </form>
                        <form>
                            <button className="employees-stats" onClick={() => navigate("/employees")}>Emploees Amount: {employeesInfo.length}</button>
                        </form>
                    </div>
                </div>
            )}
            {user.role_id === 4 && (
                <p>superadmin</p>
                // Super admin dashboard
            )}
        </main>
    );
};



export default Dashboard;
