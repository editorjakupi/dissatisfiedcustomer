import { useNavigate, Navigate } from "react-router-dom";
import './main.css';
import { useEffect, useState } from "react";

const Dashboard = ({ user, setUser }) => {
    const navigate = useNavigate();

    const [employeesInfo, setEmployeesInfo] = useState([]);
    useEffect(() => {
        if (!user?.role_id) return; // Ensure user.role_id is defined before making the request

        fetch(`/api/employees/${user.role_id}`)
            .then(response => {
                if (!response.ok) throw new Error("Failed to fetch employees");
                return response.json();
            })
            .then(data => setEmployeesInfo(data))
            .catch(error => console.error("Error fetching employees:", error));
    }, [user]); // Runs only when 'user' changes


    /* 
    const [productsInfo, setProductsInfo] = useState([]);

    // Will be changed when session is saved on server side instead of client
    useEffect(()=>{
        fetch(`/api/products/11`) 
        .then(response => response.json())
        .then(data => {setProductsInfo(data)})
    }); 
    */


    const handleLogout = async () => {
        await fetch("/api/logout", {
            method: "POST",
            credentials: "include", // Ensure cookies are cleared
        });

        setUser(null); // Clear user data
        navigate("/");
    };


    return (
        <main id="dashboard-main">
            {user?.role_id === 1 && (
                <p>Customer</p>
                // Navigate to dashboard equivelant for customers
            )}
            {user?.role_id === 2 && (
                <Navigate to="/tickets" />
            )}
            {user?.role_id === 3 && (
                <div className="admin-dashboard-div">
                    <div className="admin-dashboard-content-div">
                        <form>
                            <button className="product-stats" onClick={() => navigate("/products")}>Product Amount: {/*{productsInfo.length}*/}</button>
                        </form>
                        <form>
                            <button className="employees-stats" onClick={() => navigate("/employees")}>Employees Amount: {employeesInfo.length}</button>
                        </form>
                    </div>
                </div>
            )}
            {user?.role_id === 4 && (
                <p>Superadmin</p>
                // Super admin dashboard
            )}
        </main>
    );
};



export default Dashboard;
