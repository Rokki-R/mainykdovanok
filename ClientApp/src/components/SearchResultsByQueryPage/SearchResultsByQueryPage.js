import React, { useRef, useEffect, useState } from 'react';
import { useParams, useLocation, useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Carousel, Spinner } from 'react-bootstrap';
import axios from 'axios';
import './SearchResultsByQueryPage.css';

function SearchResultsByQueryPage() {
  const [devices, setDevices] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    async function fetchDevices() {
      const response = await axios.get('/api/device/search', {
        params: {
          searchWord: location.state.searchQuery
        }
      });
      setDevices(response.data);
    }
    fetchDevices();
  }, [searchQuery]);

  const handleOpen = (deviceId) => {
    navigate(`/skelbimas/${deviceId}`);
  };

  const handleSearch = (event) => {
    event.preventDefault();
    const searchQuery = event.target.elements.searchQuery.value;
    setSearchQuery(searchQuery);
  };
  return (
    <Container className="home">
      <form onSubmit={handleSearch}>
      </form>
      {location.state && location.state.searchResults && location.state.searchResults.length > 0 ? (
        <Row className="justify-content-center">
            <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Rasti prietaisai pagal "{location.state.searchQuery}" raktažodį</h3>
          {location.state.searchResults.map((device) => (
            <Col sm={4} key={device.id} style={{ width: '300px' }}>
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
                  <Card.Text>{device.description}</Card.Text>
                  <ul className="list-group list-group-flush mb-3">
                    <li className="list-group-item d-flex justify-content-between align-items-center">
                      <span>{device.type}</span>
                      <span>{device.location}</span>
                    </li>
                    <li className="list-group-item d-flex justify-content-between align-items-center">
                      <span>Baigiasi:</span>
                      <span>{new Date(device.endDateTime).toLocaleString('lt-LT').slice(5, -3)}</span>
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
            ) : (
                <h3 style={{ textAlign: "center", marginTop: "50px" }}>Nerasta jokių prietaisų pagal "{location.state.searchQuery}" raktažodį</h3>
            )}
        </Container>
    );
}

export default SearchResultsByQueryPage;
