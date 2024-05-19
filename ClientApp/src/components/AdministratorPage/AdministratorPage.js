import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Container, Button, Spinner } from "react-bootstrap";
import axios from "axios";
import toast, { Toaster } from "react-hot-toast";
import "./AdministratorPage.css";

function AdministratorPage() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const fetchUsers = async () => {
    try {
      const response = await axios.get("/api/admin/getUsers");
      setUsers(response.data);
      setLoading(false);
    } catch (error) {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const updateUserStatus = async (userId, action) => {
    try {
      const response = await axios.post(
        `/api/admin/updateUserStatus/${userId}?action=${action}`
      );
      if (response.status === 200) {
        fetchUsers();
        toast.success("Naudotojo būsena sėkmingai atnaujinta!");
      }
    } catch (error) {
      toast.error("Naudotojo būsenos atnaujinimas nesėkmingas ");
    }
  };

  const handleActionClick = async (userId, action) => {
    try {
      if (action === "Atblokuoti" || action === "Blokuoti") {
        await updateUserStatus(userId, action);
      }
    } catch (error) {
      console.error("Error performing action:", error);
    }
  };

  return (
    <Container className="my-5 admin">
      <div className="user-table-container">
        <Toaster />
        {loading ? (
          <Spinner animation="border" role="status" />
        ) : (
          <table className="user-table">
            <thead>
              <tr>
                <th>Vardas</th>
                <th>Pavardė</th>
                <th>El. paštas</th>
                <th>Laimėtų prietaisų kiekis</th>
                <th>Padovanotų prietaisų kiekis</th>
                <th>Būsena</th>
                <th>Veiksmas</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.name}</td>
                  <td>{user.surname}</td>
                  <td>{user.email}</td>
                  <td>{user.devicesWon}</td>
                  <td>{user.devicesGifted}</td>
                  <td>{user.status}</td>
                  <td>
                    {user.status === "Neužblokuotas" ? (
                      <Button
                        variant="danger"
                        onClick={() => handleActionClick(user.id, "Blokuoti")}
                      >
                        Blokuoti
                      </Button>
                    ) : (
                      <Button
                        variant="success"
                        onClick={() => handleActionClick(user.id, "Atblokuoti")}
                      >
                        Atblokuoti
                      </Button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </Container>
  );
}

export default AdministratorPage;
