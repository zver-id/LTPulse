import {BrowserRouter, Route, Routes} from "react-router-dom"
import Main from "../mainScreen/MainScreen.tsx";

const Router = () => {
  return <BrowserRouter>
    <Routes>
      <Route element={<Main/>} path={"/"}/>
    </Routes>
  </BrowserRouter>
}

export default Router