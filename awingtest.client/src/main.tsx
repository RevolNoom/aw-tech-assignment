import React from "react";
import ReactDOM from 'react-dom/client'
import './index.css'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { TreasureMapAdd } from './treasure_map/Add.tsx'
import { TreasureMapEdit } from './treasure_map/Edit.tsx'
import { TreasureMapDisplay } from "./treasure_map/Display.tsx";

const root = document.getElementById("root")!;

ReactDOM.createRoot(root).render(
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<TreasureMapAdd />} />
                <Route path="/treasure_map/add" element={<TreasureMapAdd />} />
                <Route path="/treasure_map/display/:id" element={<TreasureMapDisplay />} />
                <Route path="/treasure_map/edit/:id" element={<TreasureMapEdit />} />
            </Routes>
    </BrowserRouter>
        ,
)