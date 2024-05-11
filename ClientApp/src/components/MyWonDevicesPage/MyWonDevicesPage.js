import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Spinner } from 'react-bootstrap';
import axios from 'axios';
import toast from 'react-hot-toast';
import './MyWonDevicesPage.css';

function LoadingSpinner() {
  return (
    <div className="text-center mt-5">
      <Spinner animation="border" role="status">
        <span className="sr-only">Kraunama...</span>
      </Spinner>
    </div>
  );
}

function MyWonDevicesPage() {
  const [devices, setDevices] = useState(null);
  const [loading, setLoading] = useState(true);
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
        const response = await axios.get('/api/device/getUserWonDevices');
        setDevices(response.data);
        setLoading(false);
      } catch (error) {
        console.error('Error fetching user devices:', error);
        setLoading(false);
      }
    }
    fetchDevices();
  }, []);

  const handleOpen = (deviceId) => {
    navigate(`/laimejimas/${deviceId}`);
  };

  return (
    <Container className="home-user-won-devices">
      {loading ? (
        <LoadingSpinner />
      ) : devices ? (
        <>
          {devices.length > 0 && (
            <h3 style={{ textAlign: 'center', marginBottom: '10px' }}>Jūsų laimėti elektronikos prietaisų skelbimai</h3>
          )}
          {devices.length === 0 ? (
            <h3 style={{ textAlign: 'center', marginBottom: '10px' }}>Jūs dar nesate laimėjęs elektronikos prietaisų</h3>
          ) : (
            <Row className="justify-content-center">
              {devices.map((device) => (
                <Col sm={4} key={device.id} style={{ width: '300px' }}>
                  <Card className="mb-4">
                    <img
                      className="d-block w-100"
                      style={{ objectFit: 'cover' }}
                      height="256"
                      src={device.images && device.images.length > 0 ? `data:image/png;base64,${device.images[0].data}` : ''}
                      alt={device.name}
                    />
                    <Card.Body>
                      <Card.Title>{device.name}</Card.Title>
                      <Card.Text>{device.description}</Card.Text>
                      <ul className="list-group list-group-flush mb-3">
                        <li className="list-group-item d-flex justify-content-between align-items-center">
                          <span>{device.type}</span>
                          <span>{device.location}</span>
                        </li>
                      </ul>
                      <div className="d-flex justify-content-end">
                        <Button variant="primary" onClick={() => handleOpen(device.id)}>Peržiūrėti</Button>
                      </div>
                    </Card.Body>
                  </Card>
                </Col>
              ))}
            </Row>
          )}
        </>
      ) : (
        <Container className="my-5">
          <div className='outerBoxWrapper d-flex justify-content-center'>
            <Spinner animation="border" role="status" />
          </div>
        </Container>
      )}
    </Container>
  );
}

export default MyWonDevicesPage;
