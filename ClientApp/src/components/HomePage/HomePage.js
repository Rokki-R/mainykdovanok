import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Container,
  Row,
  Col,
  Card,
  Button,
  Spinner,
  Pagination,
  Form,
  Dropdown,
} from "react-bootstrap";
import axios from "axios";
import "./HomePage.css";

function HomePage() {
  const [devices, setDevices] = useState(null);
  const [categories, setCategories] = useState([]);
  const [types, setTypes] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(6);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("Visos kategorijos");
  const [selectedType, setSelectedType] = useState("Visi atidavimo būdai");
  const navigate = useNavigate();

  useEffect(() => {
    async function fetchDevices() {
      try {
        const response = await axios.get("/api/home/getDevices");
        setDevices(response.data);
      } catch (error) {
        console.error("Error fetching devices:", error);
      }
    }
    fetchDevices();
  }, []);

  useEffect(() => {
    async function fetchCategories() {
      try {
        const response = await axios.get("/api/device/getCategories");
        setCategories(response.data);
      } catch (error) {
        console.error("Error fetching categories:", error);
      }
    }
    fetchCategories();
  }, []);

  useEffect(() => {
    async function fetchDeviceTypes() {
      try {
        const response = await axios.get("/api/device/getDeviceTypes");
        setTypes(response.data);
      } catch (error) {
        console.error("Error fetching device types:", error);
      }
    }
    fetchDeviceTypes();
  }, []);

  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;

  const filteredDevices = devices
    ? devices.filter(
        (device) =>
          device.name.toLowerCase().includes(searchTerm.toLowerCase()) &&
          (selectedCategory === "Visos kategorijos" ||
            device.category === selectedCategory) &&
          (selectedType === "Visi atidavimo būdai" || device.type === selectedType)
      )
    : [];

  // Log the count of filtered devices to the console
  console.log("Number of devices found:", filteredDevices.length);

  const currentDevices = filteredDevices.slice(
    indexOfFirstItem,
    indexOfLastItem
  );

  const totalPages = devices
    ? Math.ceil(filteredDevices.length / itemsPerPage)
    : 0;

  const contentWrapperClass =
    currentDevices.length > 0 ? "content-wrapper" : "content-wrapper-centered";

  const handleOpen = (deviceId) => {
    navigate(`/skelbimas/${deviceId}`);
  };

  const handlePageChange = (page) => {
    setCurrentPage(page);
  };

  const handleSearch = (e) => {
    setSearchTerm(e.target.value);
    setCurrentPage(1);
  };

  const handleCategorySelect = (category) => {
    setSelectedCategory(category);
    setCurrentPage(1);
  };

  const handleTypeSelect = (type) => {
    setSelectedType(type);
    setCurrentPage(1);
  };

  return (
    <Container className="home">
      <Row className="justify-content-center">
        <Col sm={12}>
          <h3 style={{ textAlign: "center", marginBottom: "25px" }}>
            Elektronikos prietaisų skelbimai
          </h3>
        </Col>
      </Row>
      <Row className="justify-content-center">
        <Col sm={6}>
          <Form.Group controlId="formSearch">
            <Form.Control
              type="text"
              placeholder="Įveskite skelbimo pavadinimą"
              value={searchTerm}
              onChange={handleSearch}
            />
          </Form.Group>
        </Col>
        <Col sm={3} style={{ marginRight: "10px" }}>
          <Dropdown>
            <Dropdown.Toggle variant="secondary" id="dropdown-basic">
              {selectedCategory}
            </Dropdown.Toggle>
            <Dropdown.Menu>
              <Dropdown.Item
                onClick={() => handleCategorySelect("Visos kategorijos")}
              >
                Visos kategorijos
              </Dropdown.Item>
              <hr />
              {categories.map((category) => (
                <Dropdown.Item
                  key={category.id}
                  onClick={() => handleCategorySelect(category.name)}
                >
                  {category.name}
                </Dropdown.Item>
              ))}
            </Dropdown.Menu>
          </Dropdown>
        </Col>
        <Col sm={3}>
          <Dropdown>
            <Dropdown.Toggle variant="secondary" id="dropdown-basic">
              {selectedType}
            </Dropdown.Toggle>
            <Dropdown.Menu>
              <Dropdown.Item onClick={() => handleTypeSelect("Visi atidavimo būdai")}>
                Visi atidavimo būdai
              </Dropdown.Item>
              <hr />
              {types.map((type) => (
                <Dropdown.Item
                  key={type.id}
                  onClick={() => handleTypeSelect(type.name)}
                >
                  {type.name}
                </Dropdown.Item>
              ))}
            </Dropdown.Menu>
          </Dropdown>
        </Col>
      </Row>

      <Row className="justify-content-center">
        <Col sm={12}>
          <div className={contentWrapperClass}>
            <Row className="justify-content-center">
              {currentDevices.length > 0 ? (
                currentDevices.map((device) => (
                  <Col sm={4} key={device.id} style={{ width: "350px" }}>
                    <Card className="mb-4">
                      <img
                        className="d-block w-100"
                        style={{ objectFit: "cover" }}
                        height="256"
                        src={
                          device.images && device.images.length > 0
                            ? `data:image/png;base64,${device.images[0].data}`
                            : ""
                        }
                        alt={device.name}
                      />
                      <Card.Body>
                        <Card.Title>{device.name}</Card.Title>
                        <hr></hr>
                        <Card.Text>Atidavimo būdas: {device.type}</Card.Text>
                        <Card.Text>Vietovė: {device.location}</Card.Text>
                        <div className="d-flex justify-content-end">
                          <Button
                            variant="primary"
                            onClick={() => handleOpen(device.id)}
                          >
                            Peržiūrėti
                          </Button>
                        </div>
                      </Card.Body>
                    </Card>
                  </Col>
                ))
              ) : (
                <Col sm={12}>
                  <h3 style={{ textAlign: "center" }}>
                    {searchTerm ||
                    selectedCategory !== "Visos kategorijos" ||
                    selectedType !== "Visi atidavimo būdai"
                      ? "Nerasta elektronikos prietaisų pagal paieškos kriterijus"
                      : "Šiuo metu nėra dovanojamų elektronikos prietaisų"}
                  </h3>
                </Col>
              )}
            </Row>
          </div>
        </Col>
      </Row>
      {totalPages > 1 && (
        <Pagination className="justify-content-center">
          {[...Array(totalPages)].map((_, index) => (
            <Pagination.Item
              key={index + 1}
              active={index + 1 === currentPage}
              onClick={() => handlePageChange(index + 1)}
              style={{ color: "#0d3c34", borderColor: "#0d3c34" }}
            >
              {index + 1}
            </Pagination.Item>
          ))}
        </Pagination>
      )}
    </Container>
  );
}

export default HomePage;
