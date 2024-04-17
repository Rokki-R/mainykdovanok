import React, { useRef, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Carousel, Spinner } from 'react-bootstrap';
import axios from 'axios';
import toast, { Toaster } from 'react-hot-toast';
import './MyDevicesPage.css';


function MyDevicesPage() {
    const [devices, setDevices] = useState(null);
    const navigate = useNavigate();


    useEffect(() => {
        const fetchUserLogin = async () => {
            try {
                const response = await axios.get('api/login/isloggedin');
                if (response.status === 200) {
                    console.log('User is logged in.');
                }
            } catch (error) {
                if (error.response.status === 401) {
                    toast.error('Jūs turite būti prisijungęs');
                    navigate('/prisijungimas');
                } else {
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            }
        };
        fetchUserLogin();
    }, []);

    useEffect(() => {
        async function fetchDevices() {
            try {
                const response = await axios.get('/api/device/getUserDevices');
                setDevices(response.data);
            } catch (error) {
                console.error('Error fetching user devices:', error);
            }
        }
        fetchDevices();
    }, []);

    const handleOpen = (deviceId) => {
        navigate(`/skelbimas/${deviceId}`);
    }

    return devices ? (
        <Container className="home">
            {devices.length > 0 && (
                <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Jūsų aktyvūs skelbimai</h3>
            )}
            {devices.length === 0 ? (
                <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Jūs neturite nei vieno aktyvaus skelbimo</h3>
            ) : (
                <Row className="justify-content-center">
                    {devices.map((device) => (
                        <Col sm={4} key={device.id} style={{ width: '350px' }}>
                            <Card className="mb-4">
                                <img
                                    className="d-block w-100"
                                    style={{ objectFit: "cover" }}
                                    height="256"
                                    src={device.images && device.images.length > 0 ? `data:image/png;base64,${device.images[0].data}` : ""}
                                    alt={device.name}
                                />
                               <Card.Body>
                                            <Card.Title>{device.name}</Card.Title>
                                            <hr></hr>
                                            <Card.Text>Atidavimo būdas: {device.type}</Card.Text>
                                            <Card.Text>Vietovė: {device.location}</Card.Text>
                                            <div className="d-flex justify-content-end">
                                                <Button variant="primary" onClick={() => handleOpen(device.id)}>Peržiūrėti</Button>
                                            </div>
                                </Card.Body>
                            </Card>
                        </Col>
                    ))}
                </Row>
            )}
        </Container>
    ) : (
        <Container className="my-5">
            <div className='outerBoxWrapper d-flex justify-content-center'>
                <Spinner animation="border" role="status" />
            </div>
        </Container>
    );
}
export default MyDevicesPage;
