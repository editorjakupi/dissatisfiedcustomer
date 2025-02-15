const AdminDashboard = ({ user, setUser }) => {
  const navigate = useNavigate();

  const dashboard = <div>
    <div id='admin-statistics'>
      <div className='product-information-div'>
      </div>
      <div className='employe-information-div'>
      </div>
    </div>
  </div>

  return <div>{dashboard}</div>
};

export default AdminDashboard;
