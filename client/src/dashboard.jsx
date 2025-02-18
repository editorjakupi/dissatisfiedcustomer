
const Dashboard = ({ user }) => {

    return (
        <div className="dashboard-container">
            <h2>Welcome, {user?.name}!</h2>
            <p>Email: {user?.email}</p>
            <p>Phone: {user?.phoneNumber}</p>
            <p>Role ID: {user?.roleId}</p>
        </div>
    );
};

export default Dashboard;
