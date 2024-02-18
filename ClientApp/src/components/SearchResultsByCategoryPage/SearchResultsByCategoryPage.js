import React, { useRef, useEffect, useState } from 'react';
import { useParams, useLocation, useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Carousel, Spinner } from 'react-bootstrap';
import axios from 'axios';
import './SearchResultsByCategoryPage.css';

function SearchResultsByCategoryPage() {
  const { categoryId } = useParams();  
  const [items, setItems] = useState(null);
  const [loading, setLoading] = useState(true);
  const [category, setCategory] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    async function fetchItems() {
    setLoading(true);
    
    if (categoryId != 0)
    {
        const response = await axios.get(`/api/item/search/category/${categoryId}`);
        setItems(response.data);
        setLoading(false);
    }
    else
    {
        const response = await axios.get('/api/item/getItems');
        setItems(response.data); 
        setLoading(false);
    }
  }
  fetchItems();
}, [categoryId]);

  useEffect(() => {
    async function fetchCategory()
    {
      if (categoryId != 0)
      { 
        const response = await axios.get(`/api/item/category/${categoryId}`);
        setCategory(response.data);
      }
      else
      {
        setCategory('visi')
      }
    }
    fetchCategory();
}, [categoryId]);

  const handleOpen = (itemId) => {
    navigate(`/skelbimas/${itemId}`);
  };

  return items && !loading ? (
    <Container className="home">
        <Row className="justify-content-center">
            {items.length == 0 ? (
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
        {items.map((item) => (
       <Col sm={4} key={item.id} style={{ width: '300px' }}>
            <Card className="mb-4">
                            <img
                                className="d-block w-100"
                                style={{ objectFit: "cover" }}
                                height="256"
                                src={item.images && item.images.length > 0 ? `data:image/png;base64,${item.images[0].data}` : ""}
                                alt={item.name}
                            />
            <Card.Body>
                <Card.Title>{item.name}</Card.Title>
                <Card.Text>{item.description}</Card.Text>
                <ul className="list-group list-group-flush mb-3">
                <li className="list-group-item d-flex justify-content-between align-items-center">
                    <span>{item.type}</span>
                    <span>{item.location}</span>
                </li>
                <li className="list-group-item d-flex justify-content-between align-items-center">
                    <span>Baigiasi:</span>
                    <span>{new Date(item.endDateTime).toLocaleString('lt-LT').slice(5, -3)}</span>
                                    </li>
                                </ul>
                                <div className="d-flex justify-content-end">
                                    <Button variant="primary" onClick={() => handleOpen(item.id)}>Peržiūrėti</Button>
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
