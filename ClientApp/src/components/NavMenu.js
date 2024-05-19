import { useState, React, Component } from "react";
import {
  Navbar,
  Nav,
  NavDropdown,
  Form,
  FormControl,
  Button,
  Image,
  Collapse,
} from "react-bootstrap";
import { Link, useNavigate } from "react-router-dom";
import { withRouter } from "./withRouter";
import axios from "axios";
import toast, { Toaster } from "react-hot-toast";
import "./NavMenu.css";

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor(props) {
    super(props);
    this.state = {
      searchQuery: "",
      isClicked: false,
      isLogged: false,
      isLoggedIn: false,
      dropdownOpen: false,
      selectedCategory: "Filtras",
      categories: [],
      devices: [],
      allDevices: false,
    };
    this.handleClick = this.handleClick.bind(this);
  }

  componentDidMount() {
    this.handleLoginClick();
    this.getCategories();
  }

  getCategories() {
    axios
      .get("api/device/getCategories")
      .then((response) => {
        this.setState({
          categories: response.data,
        });
      })
      .catch((error) => {
        console.log(error);
      });
  }

  handleClick() {
    this.setState({
      isClicked: !this.state.isClicked,
    });
  }

  handleSearchInputChange = (event) => {
    this.setState({ searchQuery: event.target.value });
  };

  handleSearch = () => {
    if (!this.state.searchQuery) {
      toast.error("Negalite ieškoti skelbimų nieko neįvedę paieškos lange");
      return;
    }

    axios
      .get("/api/device/search", {
        params: {
          searchWord: this.state.searchQuery,
        },
      })
      .then((response) => {
        this.props.navigate(`/search/${this.state.searchQuery}`, {
          state: {
            searchResults: response.data,
            searchQuery: this.state.searchQuery,
          },
        });
      })
      .catch((error) => {
        console.error(error);
      });
  };

  getDevicesByCategory(categoryId) {
    this.props.navigate(`/search/category/${categoryId}`);
  }

  getAllDevices = () => {
    this.setState({
      allDevices: true,
    });
    this.props.navigate(`/search/category/0`);
  };

  handleLoginClick = () => {
    this.setState({
      isClicked: !this.state.isClicked,
    });

    fetch("api/login/isloggedin/", { method: "GET" })
      .then((response) => {
        if (response.ok) {
          return response.json();
        } else {
          throw new Error("User is not logged in");
        }
      })
      .then((data) => {
        const userRole = data.userRole;
        const userEmail = data.userEmail;
        this.setState({
          isLogged: true,
          userRole: userRole,
          userEmail: userEmail,
        });
      })
      .catch((error) => {
        console.error("Error checking login status:", error);
      });
  };

  handleLogoutClick = () => {
    fetch("api/login/logout", { method: "GET" }).then((response) => {
      if (response.status === 200) {
        this.setState({ isLogged: false });
        window.location.reload();
        window.location.href = "/prisijungimas";
      } else if (response.status === 401) {
        toast.error("Jūs jau esate atsijungęs!");
      }
    });
  };

  selectCategory = (category) => {
    this.setState({
      selectedCategory: category,
    });
  };

  toggleDropdown = () => {
    this.setState((prevState) => ({
      dropdownOpen: !prevState.dropdownOpen,
    }));
  };

  render() {
    const { userRole, isLogged } = this.state;
    const maxCategoryLength = 20;
    let displayCategory = this.state.selectedCategory;
    if (displayCategory.length > maxCategoryLength) {
      displayCategory = displayCategory.substring(0, maxCategoryLength) + "...";
    }

    return (
      <>
        <Toaster />
        <Navbar
          style={{ backgroundColor: "#c3d5c7", height: "95px" }}
          expand="lg"
          sticky="top"
        >
          <Navbar.Brand>
            <Link to="/">
              <Image
                src="/images/logo-green.png"
                alt="Logo"
                style={{ height: "60px", width: "auto", marginLeft: "5px" }}
              />
            </Link>
          </Navbar.Brand>

          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse
            id="basic-navbar-nav"
            style={{ backgroundColor: "#c3d5c7" }}
          >
            <Nav className="ms-auto">
              {this.state.isLogged && (
                <Nav className="userEmail d-flex align-items-center">
                  <span>Prisijungęs: {this.state.userEmail}</span>
                </Nav>
              )}
              {userRole === 0 ? (
                <div className="d-flex align-items-center">
                  <Link className="nav-link me-2" to="/manoskelbimai">
                    Mano skelbimai
                  </Link>
                  <Link className="nav-link me-2" to="/laimejimai">
                    Laimėti skelbimai
                  </Link>
                  <Link className="nav-link me-2" to="/manoprofilis">
                    Mano profilis
                  </Link>
                  <Button
                    className="buttoncreate"
                    variant="primary"
                    size="sm"
                    href="/naujasskelbimas"
                  >
                    Dovanoti!
                  </Button>
                </div>
              ) : userRole === 1 ? (
                <Link className="nav-link" to="/admin">
                  Naudotojai
                </Link>
              ) : (
                <div className="d-flex align-items-center">
                  <Link className="nav-link me-2" to="/prisijungimas">
                    Prisijungti
                  </Link>
                  <Link className="nav-link me-2" to="/registracija">
                    Registruotis
                  </Link>
                </div>
              )}

              {this.state.isLogged && (
                <Link
                  className="nav-link"
                  to="/"
                  onClick={this.handleLogoutClick}
                >
                  Atsijungti
                </Link>
              )}
            </Nav>
          </Navbar.Collapse>
        </Navbar>
      </>
    );
  }
}

export function Redirection(props) {
  const navigate = useNavigate();
  return <NavMenu navigate={navigate}></NavMenu>;
}

export default withRouter(NavMenu);
