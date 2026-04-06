import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type { ITicket } from "../../components/ticketsScreen/ticketScreen.tsx";

export const ticketsApi = createApi({
    reducerPath: "tickets",
    baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api`}),
    tagTypes: ['tickets'],
    endpoints: (builder) => ({
        getTickets: builder.query<ITicket[], { teamId: number, date: string, ticketType: string}>({
            query: ({teamId, date, ticketType})=>
                `Tickets?teamId=${teamId}&date=${date}&ticketType=${ticketType}`,
            providesTags: ['tickets']
        }),
        updateTicket: builder.mutation({
            query: (ticket: ITicket) =>({
                url: 'Tickets',
                method: 'POST',
                body: ticket
            }),
            invalidatesTags: ['tickets']
        })
    })
});

export const { useGetTicketsQuery,
    useUpdateTicketMutation } = ticketsApi;