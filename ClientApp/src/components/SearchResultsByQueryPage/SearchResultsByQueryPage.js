import React, { useRef, useEffect, useState } from 'react';
import { useParams, useLocation, useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Carousel, Spinner } from 'react-bootstrap';
import axios from 'axios';
import './SearchResultsByQueryPage.css';

function SearchResultsByQueryPage() {
  const [items, setItems] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    async function fetchItems() {
      const response = await axios.get('/api/item/search', {
        params: {
          searchWord: location.state.searchQuery
        }
      });
      setItems(response.data);
    }
    fetchItems();
  }, [searchQuery]);

  const handleOpen = (itemId) => {
    navigate(`/skelbimas/${itemId}`);
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
          {location.state.searchResults.map((item) => (
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
            ) : (
                <h3 style={{ textAlign: "center", marginTop: "50px" }}>Nerasta jokių prietaisų pagal "{location.state.searchQuery}" raktažodį</h3>
            )}
        </Container>
    );
}

export default SearchResultsByQueryPage;
