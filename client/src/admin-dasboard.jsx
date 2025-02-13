import { useNavigate } from "react-router-dom";

const AdminDashboard = ({ user, setUser }) => {
  const navigate = useNavigate();

  return <main>
    <div id='admin-statistics'>
      <div className='product-information-div'>
      </div>
      <div className='employe-information-div'>
      </div>
    </div>
  </main>
};

export default AdminDashboard;
