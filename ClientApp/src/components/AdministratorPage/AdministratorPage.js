import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Spinner, Table } from 'react-bootstrap';
import axios from 'axios';
import toast, { Toaster } from 'react-hot-toast';
import './AdministratorPage.css';

function AdministratorPage() {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    const fetchUsers = async () => {
        try {
            const response = await axios.get('/api/admin/getUsers');
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
            const response = await axios.post(`/api/admin/updateUserStatus/${userId}?action=${action}`);
            if (response.status === 200) {
                fetchUsers();
                toast.success('Naudotojo būsena sėkmingai atnaujinta!');
            }
        } catch (error) {
            toast.error('Naudotojo būsenos atnaujinimas nesėkmingas ');
        }
    };

    const handleActionClick = async (userId, action) => {
        try {
            if (action === 'Atblokuoti' || action === 'Blokuoti') {
                await updateUserStatus(userId, action);
            }
        } catch (error) {
            console.error('Error performing action:', error);
        }
    };

    return (
        <Container className="admin">
            <Row>
                <Col>
                    <Card>
                        <Card.Header as="h5">Naudotojai</Card.Header>
                        <Card.Body>
                            {loading ? (
                                <Spinner animation="border" role="status" />
                            ) : (
                                <Table striped bordered hover>
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
                                        {users.map(user => (
                                            <tr key={user.id}>
                                                <td className="align-middle">{user.name}</td>
                                                <td className="align-middle">{user.surname}</td>
                                                <td className="align-middle">{user.email}</td>
                                                <td className="align-middle">{user.devicesWon}</td>
                                                <td className="align-middle">{user.devicesGifted}</td>
                                                <td className="align-middle">{user.status}</td>
                                                <td className="align-middle">
                                                    {user.status === 'Neužblokuotas' ? (
                                                        <Button variant="danger" onClick={() => handleActionClick(user.id, 'Blokuoti')}>Blokuoti</Button>
                                                    ) : (
                                                        <Button variant="success" onClick={() => handleActionClick(user.id, 'Atblokuoti')}>Atblokuoti</Button>
                                                    )}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </Table>
                            )}
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
}

export default AdministratorPage;
