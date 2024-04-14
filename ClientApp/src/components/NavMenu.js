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
        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
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
        console.log(userRole);
        this.setState({ isLogged: true, userRole: userRole });
      })
      .catch((error) => {
        console.error("Error checking login status:", error);
      });
  };

  handleLogoutClick = () => {
    fetch("api/login/logout", { method: "GET" }).then((response) => {
      if (response.status === 200) {
        // 200 - Ok
        this.setState({ isLogged: false });
        window.location.reload();
        window.location.href = "/prisijungimas";
      } else if (response.status === 401) {
        // 401 - Unauthorized
        toast.error("Jūs jau esate atsijungęs!");
      } else {
        // 500 - Internal server error
        toast.error("Įvyko klaida, susisiekite su administratoriumi!");
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
    const { userRole } = this.state;
    const maxCategoryLength = 20;
    let displayCategory = this.state.selectedCategory;
    if (displayCategory.length > maxCategoryLength) {
      displayCategory =
        displayCategory.substring(0, maxCategoryLength) + "...";
    }

    return (
      <>
        <Toaster />
        <Navbar
          style={{ backgroundColor: "#c3d5c7", height: "95px" }}
          expand="lg"
          sticky="top"
        >
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse
            id="basic-navbar-nav"
            style={{ backgroundColor: "#c3d5c7" }}
          >
            <Form inline className="d-flex">
              <FormControl
                style={{
                  width: "250px",
                  height: "50px",
                  margin: "5px 0px 0px 0px",
                }}
                type="text"
                placeholder="Įveskite..."
                value={this.state.searchQuery}
                onChange={this.handleSearchInputChange}
              />
              <Button
                className="buttonsearch"
                onClick={this.handleSearch}
              >
                Ieškoti
              </Button>
            </Form>
            <Nav className="ms-auto">
              <NavDropdown
                title={displayCategory}
                className="categories"
              >
                {this.state.categories.map((category) => (
                  <NavDropdown.Item
                    key={category.id}
                    onClick={() => this.getDevicesByCategory(category.id)}
                  >
                    {category.name}
                  </NavDropdown.Item>
                ))}
                <NavDropdown.Divider />
                <NavDropdown.Item onClick={() => this.getAllDevices()}>
                  Visi
                </NavDropdown.Item>
              </NavDropdown>
              {userRole === 0 && (
                <div className="d-inline-block align-middle">
                  <Button
                    className="buttoncreate"
                    variant="primary"
                    size="sm"
                    href="/naujasskelbimas"
                  >
                    Dovanoti!
                  </Button>
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
        <footer>
  <div className="links">
    {this.state.isLogged ? (
      <>
        {this.state.userRole === 0 && (
          <Link className="links" to="/manoskelbimai">
            Mano skelbimai
          </Link>
        )}
        {this.state.userRole === 0 && (
          <Link className="links" to="/laimejimai">
            Laimėti skelbimai
          </Link>
        )}
        {this.state.userRole === 1 && (
          <Link className="links" to="/admin">
            Naudotojai
          </Link>
        )}
        <div className="ml-auto">
          <Link className="links" to="/manoprofilis">
            Mano profilis
          </Link>
        </div>
      </>
    ) : (
      <>
        <Link className="links" to="/prisijungimas">
          Prisijungti
        </Link>
        <Link className="links" to="/registracija">
          Registruotis
        </Link>
      </>
    )}
  </div>
</footer>

      </>
    );
  }
}

export function Redirection(props) {
  const navigate = useNavigate();
  return <NavMenu navigate={navigate}></NavMenu>;
}

export default withRouter(NavMenu);
