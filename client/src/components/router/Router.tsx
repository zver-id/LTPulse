import {BrowserRouter, Route, Routes} from "react-router-dom"
import Main from "../mainScreen/MainScreen.tsx";
import TeamsSettings from "../teamsSettings/teamsSettings.tsx";

const Router = () => {
  return <BrowserRouter>
    <Routes>
      <Route element={<Main/>} path={"/"}/>
      <Route element={<TeamsSettings/>} path={"/teams"}/>
    </Routes>
  </BrowserRouter>
}

export default Router