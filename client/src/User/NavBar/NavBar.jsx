  import { useEffect } from "react";
  import React from "react";
  // Vi använder endast "react-router" om det krävs, men här importeras den vanligaste standarden från "react-router-dom"
  // (För webbaserad app är detta vanligtatt använda)
  import { useNavigate, useLocation } from "react-router";
  import "../../main.css";
  import logo from "../../assets/logo.png";
  
  const NavBar = ({ user, setUser }) => {
    const navigate = useNavigate();
    const location = useLocation(); // Get the current URL path

    useEffect(() => {
      // Hämtar användarinfo från sessionStorage om den existerar
      const storedUser = JSON.parse(sessionStorage.getItem("user"));
      if (storedUser) {
        setUser(storedUser);
      }
    }, [setUser]);
  
    // Vi definierar rollnamn för de användare som har konto (employees, admin, superadmin)
    const roleNames = {
      2: "Employee",
      3: "Admin",
      4: "Super Admin"
    };
  
    const handleLogout = async () => {
      await fetch("/api/logout", {
        method: "POST",
      });
      setUser(null);
      navigate("/");
    };
    
    // Function to check if a button is active
    const isActive = (path, search = "") =>
        location.pathname === path && (search === "" || location.search === search)
            ? "active"
            : "";
    
    return (
      <div className="sidebar">
        {/* Logo-sektionen - samma för alla */}
        <div className="logo-container">
          <button onClick={() => navigate("/dashboard")} className="logo-button">
            <img src={logo} alt="Dissatisfied Customer Logo" className="logo" />
          </button>
        </div>
  
        {/* Endast visa navigeringsalternativ för användare med konto (employees, admin, superadmin) */}
        {user && user.role_id !== 1 && (
            <>
              {user.role_id === 2 && (
                  <div className="nav-section">
                    <h3>Employee Panel</h3>
                    <button onClick={() => navigate("/account")} className={isActive("/account")}>My Account</button>
                    <button onClick={() => navigate("/tickets?view=all")} className={isActive("/tickets", "?view=all")}>All Tickets</button>
                    <button onClick={() => navigate("/tickets?view=open")} className={isActive("/tickets", "?view=open")}>Open Tickets</button>
                    <button onClick={() => navigate("/tickets?view=pending")} className={isActive("/tickets", "?view=pending")}>Pending Tickets</button>
                    <button onClick={() => navigate("/tickets?view=closed")} className={isActive("/tickets", "?view=closed")}>Closed Tickets</button>
                  </div>
              )}

              {user.role_id === 3 && (
                  <div className="nav-section">
                    <h3>Admin Panel</h3>
                    <button onClick={() => navigate("/account")} className={isActive("/account")}>My Account</button>
                    <button onClick={() => navigate("/dashboard")} className={isActive("/dashboard")}>Overview</button>
                    <button onClick={() => navigate("/products")} className={isActive("/products")}>Products</button>
                    <button onClick={() => navigate("/employee")} className={isActive("/employee")}>Employees</button>
                    <button onClick={() => navigate("/feedback")} className={isActive("/feedback")}>Feedback</button>
                  </div>
              )}

              {user.role_id === 4 && (
                  <div className="nav-section">
                    <h3>Super Admin Panel</h3>
                    <button onClick={() => navigate("/account")} className={isActive("/account")}>My Account</button>
                    <button onClick={() => navigate("/companies")} className={isActive("/companies")}>Companies</button>
                    <button onClick={() => navigate("/admins")} className={isActive("/admins")}>Admins</button>
                    <button onClick={() => navigate("/users")} className={isActive("/users")}>Users</button>
                  </div>
              )}
            </>
        )}
        
        {/* Kunde-panel tas bort, eftersom kunder inte har konto */}
        
        {/* Logout-knapp visas för de inloggade användarna */}
        <div className="logout-container">
          <button className="logout-button" onClick={handleLogout}>
            Logout
          </button>
        </div>
        
        {/* Användarinfo visas endast om det är en anställd/admin (inte för kunder) */}
        {user && user.role_id !== 1 && (
          <div className="user-info">
            <p><strong>{user?.name}</strong></p>
            <p>{roleNames[user?.role_id] || "Unknown Role"}</p>
          </div>
        )}
      </div>
    );
  };
  
  export default NavBar;
