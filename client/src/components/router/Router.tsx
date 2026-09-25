import {BrowserRouter, Route, Routes} from "react-router-dom"
import Main from "../mainScreen/MainScreen.tsx";
import TeamsSettings from "../teamsSettings/teamsSettings.tsx";
import EmployeeStats from "../employeeStats/employeeStats.tsx";

const Router = () => {
  return <BrowserRouter>
    <Routes>
      <Route element={<Main/>} path={"/"}/>
      <Route element={<TeamsSettings/>} path={"/teams"}/>
      <Route element={<EmployeeStats/>} path={"/employee-stats"}/>
    </Routes>
  </BrowserRouter>
}

export default Router