import React, { useRef, useEffect, useState } from 'react';
import { useParams, useLocation, useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Carousel, Spinner } from 'react-bootstrap';
import axios from 'axios';
import './SearchResultsByCategoryPage.css';

function SearchResultsByCategoryPage() {
  const { categoryId } = useParams();  
  const [devices, setDevices] = useState(null);
  const [loading, setLoading] = useState(true);
  const [category, setCategory] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    async function fetchDevices() {
    setLoading(true);
    
    if (categoryId != 0)
    {
        const response = await axios.get(`/api/device/search/category/${categoryId}`);
        setDevices(response.data);
        setLoading(false);
    }
    else
    {
        const response = await axios.get('/api/home/getDevices');
        setDevices(response.data); 
        setLoading(false);
    }
  }
  fetchDevices();
}, [categoryId]);

  useEffect(() => {
    async function fetchCategory()
    {
      if (categoryId != 0)
      { 
        const response = await axios.get(`/api/device/category/${categoryId}`);
        setCategory(response.data);
      }
      else
      {
        setCategory('visi')
      }
    }
    fetchCategory();
}, [categoryId]);

  const handleOpen = (deviceId) => {
    navigate(`/skelbimas/${deviceId}`);
  };

  return devices && !loading ? (
    <Container className="home">
        <Row className="justify-content-center">
            {devices.length == 0 ? (
                <Container className="my-5">
                    <div className='outerBoxWrapper d-flex justify-content-center'>
                        <h3 style={{ textAlign: "center", marginTop: "50px" }}>Nerasta jokių prietaisų pagal kategorija: {category.name} </h3>
                    </div>
                </Container>
            ) : (
                categoryId != 0 ? (
                    <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Rasti prietaisai pagal kategorija: {category.name}</h3>
                ) : (
                    <h3 style={{ textAlign: "center", marginBottom: "50px" }}>Visi prietaisai esantys skelbimuose:</h3>
                )
            )}
        {devices.map((device) => (
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
    </Container>
  ) : (
    <Container className="my-5">
        <div className='outerBoxWrapper d-flex justify-content-center'>
            <Spinner animation="border" role="status" />
        </div>
    </Container>
);
}

export default SearchResultsByCategoryPage;
