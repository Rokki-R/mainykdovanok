import React, { useState } from "react";
import { useNavigate } from "react-router";
import "./PasswordChangePage.css";
import toast, { Toaster } from "react-hot-toast";
import { Form, Button, Alert, Card } from "react-bootstrap";

const PasswordChangePage = () => {
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [message, setMessage] = useState("");
  const [matchMessage, setMatchMessage] = useState("");
  const navigate = useNavigate();

  function checkFields() {
    if (
      password === confirmPassword &&
      password.length >= 8 &&
      confirmPassword.length >= 8 &&
      /^(?=.*\d)(?=.*[!@#$%^&*+\-])(?=.*[a-z])(?=.*[A-Z]).{8,}$/.test(password)
    ) {
      setMatchMessage("");
      setMessage("");
      return true;
    } else if (password.length === 0 || confirmPassword.length === 0) {
      setMatchMessage("Slaptažodžių laukai turi būti užpildyti!");
      return false;
    } else if (password !== confirmPassword) {
      setMatchMessage("Slaptažodžiai turi sutapti!");
      return false;
    } else {
      setMatchMessage(
        "Slaptažodis turi būti sudarytas bent iš 8 simbolių, turėti bent vieną didžiąją raidę, skaičių bei spec. simbolį"
      );
    }
  }

  const handleSubmit = (event) => {
    event.preventDefault();
    if (checkFields()) {
      const urlParams = new URLSearchParams(window.location.search);
      const email = urlParams.get("email");
      const token = urlParams.get("token");
      const requestOptions = {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          password: password,
          confirmPassword: confirmPassword,
          email: email,
          token: token,
        }),
      };
      fetch("api/user/changePassword", requestOptions).then((response) => {
        if (response.status === 200) {
          toast.success("Slaptažodis sėkmingai pakeistas!");
          navigate("/prisijungimas");
        } else if (response.status === 401) {
          toast.error(
            "Neteisingi nuorodos duomenys. Pakartokite slaptažodžio pakeitimo užklausą."
          );
        } else {
          toast.error("Įvyko klaida, susisiekite su administratoriumi!");
        }
      });
    }
  };

  return (
    <div className="outerPasswordBoxWrapper">
      <Card className="passwordChangeCard">
        <Toaster />
        <Card.Header className="header d-flex justify-content-between align-items-center">
          <div className="title-text">Slaptažodžio keitimas</div>
        </Card.Header>
        <Card.Body>
          <Form>
            <Form.Group controlId="password">
              <Form.Label className="label">Naujas slaptažodis</Form.Label>
              <Form.Control
                type="password"
                name="password"
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Slaptažodis"
              />
            </Form.Group>
            <Form.Group controlId="confirmPassword">
              <Form.Label className="label">
                Pakartoti naują slaptažodį
              </Form.Label>
              <Form.Control
                type="password"
                name="confirmPassword"
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Pakartokite slaptažodį"
              />
            </Form.Group>
            {message && (
              <Alert variant="danger" style={{ marginTop: "10px" }}>
                {message}
              </Alert>
            )}
            {matchMessage && (
              <Alert variant="danger" style={{ marginTop: "10px" }}>
                {matchMessage}
              </Alert>
            )}
            <div className="text-center">
              <Button
                className="change"
                type="submit"
                onClick={(event) => handleSubmit(event)}
              >
                Patvirtinti
              </Button>
            </div>
            <div className="returnToLogin">
              <a href="/prisijungimas" className="returnToLoginButton">
                Grįžti į prisijungimą
              </a>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </div>
  );
};
export default PasswordChangePage;
