import HomePage from "./components/HomePage/HomePage";

import { LoginPage } from "./components/LoginPage/LoginPage";
import RegistrationPage from "./components/RegistrationPage/RegistrationPage";
import ForgotPasswordPage from "./components/ForgotPasswordPage/ForgotPasswordPage";
import PasswordChangePage from "./components/PasswordChangePage/PasswordChangePage";
import AdministratorPage from "./components/AdministratorPage/AdministratorPage";

import DeviceCreationPage from "./components/DeviceCreationPage/DeviceCreationPage";
import { DeviceViewPage } from "./components/DeviceViewPage/DeviceViewPage";
import { DeviceParticipantsPage } from "./components/DeviceParticipantsPage/DeviceParticipantsPage";

import MyWonDevicesPage from "./components/MyWonDevicesPage/MyWonDevicesPage";
import MyProfilePage from "./components/MyProfilePage/MyProfilePage";
import MyDevicesPage from "./components/MyDevicesPage/MyDevicesPage";
import DeviceUpdatePage from "./components/DeviceUpdatePage/DeviceUpdatePage";

import { DeviceWinnerPage } from "./components/DeviceWinnerPage/DeviceWinnerPage";

const AppRoutes = [
  {
    index: true,
    element: <HomePage />,
  },
  {
    path: "/registracija",
    element: <RegistrationPage />,
  },
  {
    path: "/prisijungimas",
    element: <LoginPage />,
  },
  {
    path: "/naujasskelbimas",
    element: <DeviceCreationPage />,
  },
  {
    path: "/skelbimas/:deviceId",
    element: <DeviceViewPage />,
  },
  {
    path: "/skelbimas/info/:deviceId",
    element: <DeviceParticipantsPage />,
  },
  {
    path: "/manoprofilis",
    element: <MyProfilePage />,
  },
  {
    path: "/laimejimas/:deviceId",
    element: <DeviceWinnerPage />,
  },
  {
    path: "/manoskelbimai",
    element: <MyDevicesPage />,
  },
  {
    path: "/pamirsau-slaptazodi",
    element: <ForgotPasswordPage />,
  },
  {
    path: "/pakeisti-slaptazodi",
    element: <PasswordChangePage />,
  },
  {
    path: "/skelbimas/redaguoti/:deviceId",
    element: <DeviceUpdatePage />,
  },
  {
    path: "/laimejimai",
    element: <MyWonDevicesPage />,
  },
  {
    path: "/admin",
    element: <AdministratorPage />,
  },
];

export default AppRoutes;
