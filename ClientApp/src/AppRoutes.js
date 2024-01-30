import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import { Home } from "./components/Home";

import { LoginPage } from "./components/LoginPage/LoginPage";
import RegistrationPage from "./components/RegistrationPage/RegistrationPage";
import VerifyEmailPage from "./components/VerifyEmailPage/VerifyEmailPage";

import ItemCreationPage from "./components/ItemCreationPage/ItemCreationPage";

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/counter',
    element: <Counter />
  },
  {
    path: '/fetch-data',
    element: <FetchData />
  },
  {
    path: '/registracija',
    element: <RegistrationPage />
  },
  {
    path: '/prisijungimas',
    element: <LoginPage />
  },
  {
    path: '/verifyemail',
    element: <VerifyEmailPage />
  },
  {
    path: '/naujasskelbimas',
    element: <ItemCreationPage />
  }
];

export default AppRoutes;
