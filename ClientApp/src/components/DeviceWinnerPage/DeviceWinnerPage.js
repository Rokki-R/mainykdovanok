import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Carousel, Col, Container, Row, Button, Card, Spinner } from 'react-bootstrap';
import toast, { Toaster } from 'react-hot-toast';
import './DeviceWinnerPage.css'
import axios from 'axios';

export const DeviceWinnerPage = () => {
    const { deviceId } = useParams();
    const [viewerId, setViewerId] = useState(null);
    const [canAccess, setCanAccess] = useState(null);
    const [device, setDevice] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchViewerId = async () => {
            try {
                const response = await axios.get('api/user/getCurrentUserId');
                setViewerId(response.data);

                if (response.data === device?.winnerId && (device?.status === 'Išrinktas laimėtojas' || device?.status === 'Laukiama patvirtinimo' || device?.status === 'Atiduotas')) {
                    setCanAccess(true);
                } else if (device && response.data !== device?.winnerId) {
                    navigate('/');
                }
            } catch (error) {
                if (error.response.status === 401) {
                    navigate('/prisijungimas');
                    toast.error('Turite būti prisijungęs!');
                }
                else {
                    navigate('/index');
                    toast.error('Įvyko klaida, susisiekite su administratoriumi!');
                }
            }
        };
        fetchViewerId();
    }, [device]);

    useEffect(() => {
        const fetchDevice = async () => {
            try {
                const response = await axios.get(`api/device/getDevice/${deviceId}`);
                setDevice(response.data);
            } catch (error) {
                toast.error('Įvyko klaida, susisiekite su administratoriumi!');
            }
        };
        fetchDevice();
    }, [deviceId, canAccess]);
    
    const handleConfirm = async () => {
        try {
            const data = {
                id: deviceId,
                winnerId: device.winnerId,
                ownerId: device?.userId
            };
    
            const response = await axios.post('api/devicewinner/confirm', data);
            window.location.reload();
            toast.success('Sėkmingai patvirtinta!');
        } catch (error) {
            toast.error('Įvyko klaida patvirtinant!');
        }
    };
    

    return device && viewerId && canAccess ? (
        <div className='outerBoxWrapper'>
            <Toaster />
            <Container className="my-5">
                <Row>
                    <Col md={4}>
                        {device.images && device.images.length > 0 && (
                            <Carousel indicators={false}>
                                {device.images.map((image, index) => (
                                    <Carousel.Item key={index}>
                                        <img className="d-block w-100" 
                                        src={`data:image/png;base64,${image.data}`}
                                        alt={`Image ${index + 1}`}
                                        height="320"
                                        style={{ border: '1px solid white' }}
                                            />
                                    </Carousel.Item>
                                ))}
                            </Carousel>
                        )}
                    </Col>
                    <Col md={8}>
                        <Card>
                            <Card.Header>{device.category}</Card.Header>
                            <Card.Body>
                                <Card.Title>Laimėjote: {device.name}</Card.Title>
                                <Card.Text style={{marginTop: '30px'}}><b>Skelbimo aprašymas:</b> {device.description}</Card.Text>
                                <hr></hr>

{device.status === 'Išrinktas laimėtojas' && (
    <Button variant="primary" onClick={handleConfirm}>Patvirtinti</Button>
)}

                                <br></br>
                            </Card.Body>
                        </Card>
                    </Col>
                </Row>
            </Container>
        </div>
    ) : (
        <Container className="my-5">
            <div className='outerBoxWrapper d-flex justify-content-center'>
                <Spinner animation="border" role="status" />
            </div>
        </Container>
    );    
}