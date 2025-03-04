--
-- PostgreSQL database dump
--

-- Dumped from database version 16.4
-- Dumped by pg_dump version 16.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: public; Type: SCHEMA; Schema: -; Owner: postgres
--

-- *not* creating schema, since initdb creates it


ALTER SCHEMA public OWNER TO postgres;

--
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON SCHEMA public IS '';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: category; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.category (
    id integer NOT NULL,
    name character varying(255) NOT NULL
);


ALTER TABLE public.category OWNER TO postgres;

--
-- Name: category_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.category_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.category_id_seq OWNER TO postgres;

--
-- Name: category_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.category_id_seq OWNED BY public.category.id;


--
-- Name: company; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.company (
    id integer NOT NULL,
    company_name character varying(255) NOT NULL,
    company_phone character varying(50),
    company_email character varying(255)
);


ALTER TABLE public.company OWNER TO postgres;

--
-- Name: company_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.company_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.company_id_seq OWNER TO postgres;

--
-- Name: company_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.company_id_seq OWNED BY public.company.id;


--
-- Name: employees; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.employees (
    id integer NOT NULL,
    user_id integer,
    company_id integer
);


ALTER TABLE public.employees OWNER TO postgres;

--
-- Name: employees_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.employees_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.employees_id_seq OWNER TO postgres;

--
-- Name: employees_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.employees_id_seq OWNED BY public.employees.id;


--
-- Name: messages; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.messages (
    id integer NOT NULL,
    ticket_id integer,
    message text NOT NULL,
    user_id integer
);


ALTER TABLE public.messages OWNER TO postgres;

--
-- Name: messages_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.messages_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.messages_id_seq OWNER TO postgres;

--
-- Name: messages_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.messages_id_seq OWNED BY public.messages.id;


--
-- Name: product; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.product (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    description text,
    company_id integer
);


ALTER TABLE public.product OWNER TO postgres;

--
-- Name: product_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.product_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.product_id_seq OWNER TO postgres;

--
-- Name: product_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.product_id_seq OWNED BY public.product.id;


--
-- Name: ticket_tokens; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ticket_tokens (
    id integer NOT NULL,
    ticket_id integer NOT NULL,
    token uuid NOT NULL,
    expires_at timestamp without time zone NOT NULL
);


ALTER TABLE public.ticket_tokens OWNER TO postgres;

--
-- Name: ticket_tokens_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.ticket_tokens_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.ticket_tokens_id_seq OWNER TO postgres;

--
-- Name: ticket_tokens_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.ticket_tokens_id_seq OWNED BY public.ticket_tokens.id;


--
-- Name: tickets; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tickets (
    id integer NOT NULL,
    company_id integer,
    user_id integer,
    employee_id integer,
    product_id integer,
    category_id integer,
    date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    title character varying(255) NOT NULL,
    description text NOT NULL,
    status_id integer,
    case_number character varying,
    token character varying
);


ALTER TABLE public.tickets OWNER TO postgres;

--
-- Name: ticketstatus; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ticketstatus (
    id integer NOT NULL,
    status_name character varying NOT NULL
);


ALTER TABLE public.ticketstatus OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id integer NOT NULL,
    name character varying(255),
    email character varying(255) NOT NULL,
    password character varying(255) NOT NULL,
    phonenumber character varying(50),
    role_id integer NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: tickets_all; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.tickets_all AS
 SELECT t.id,
    t.date,
    t.title,
    c.name AS category_name,
    u.email,
    ts.status_name,
    t.case_number
   FROM (((public.tickets t
     JOIN public.category c ON ((t.category_id = c.id)))
     JOIN public.users u ON ((t.user_id = u.id)))
     JOIN public.ticketstatus ts ON ((t.status_id = ts.id)));


ALTER VIEW public.tickets_all OWNER TO postgres;

--
-- Name: tickets_closed; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.tickets_closed AS
 SELECT id,
    date,
    title,
    category_name AS name,
    email,
    status_name
   FROM public.tickets_all
  WHERE (((status_name)::text = 'Closed'::text) OR ((status_name)::text = 'Resolved'::text));


ALTER VIEW public.tickets_closed OWNER TO postgres;

--
-- Name: tickets_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.tickets_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.tickets_id_seq OWNER TO postgres;

--
-- Name: tickets_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.tickets_id_seq OWNED BY public.tickets.id;


--
-- Name: tickets_open; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.tickets_open AS
 SELECT id,
    date,
    title,
    category_name AS name,
    email,
    status_name
   FROM public.tickets_all
  WHERE (((status_name)::text = 'Unread'::text) OR ((status_name)::text = 'In Progress'::text));


ALTER VIEW public.tickets_open OWNER TO postgres;

--
-- Name: tickets_pending; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.tickets_pending AS
 SELECT id,
    date,
    title,
    category_name AS name,
    email,
    status_name
   FROM public.tickets_all
  WHERE ((status_name)::text = 'Pending'::text);


ALTER VIEW public.tickets_pending OWNER TO postgres;

--
-- Name: ticketstatus_column_name_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ticketstatus ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ticketstatus_column_name_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: userroles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.userroles (
    id integer NOT NULL,
    name character varying(50) NOT NULL
);


ALTER TABLE public.userroles OWNER TO postgres;

--
-- Name: userroles_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.userroles_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.userroles_id_seq OWNER TO postgres;

--
-- Name: userroles_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.userroles_id_seq OWNED BY public.userroles.id;


--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: userxcompany; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.userxcompany AS
 SELECT u.id,
    u.email,
    u.name,
    u.password,
    u.phonenumber,
    u.role_id,
    e.company_id AS companyid
   FROM (public.users u
     JOIN public.employees e ON ((u.id = e.user_id)));


ALTER VIEW public.userxcompany OWNER TO postgres;

--
-- Name: category id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.category ALTER COLUMN id SET DEFAULT nextval('public.category_id_seq'::regclass);


--
-- Name: company id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.company ALTER COLUMN id SET DEFAULT nextval('public.company_id_seq'::regclass);


--
-- Name: employees id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees ALTER COLUMN id SET DEFAULT nextval('public.employees_id_seq'::regclass);


--
-- Name: messages id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages ALTER COLUMN id SET DEFAULT nextval('public.messages_id_seq'::regclass);


--
-- Name: product id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product ALTER COLUMN id SET DEFAULT nextval('public.product_id_seq'::regclass);


--
-- Name: ticket_tokens id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ticket_tokens ALTER COLUMN id SET DEFAULT nextval('public.ticket_tokens_id_seq'::regclass);


--
-- Name: tickets id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets ALTER COLUMN id SET DEFAULT nextval('public.tickets_id_seq'::regclass);


--
-- Name: userroles id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.userroles ALTER COLUMN id SET DEFAULT nextval('public.userroles_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Data for Name: category; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.category (id, name) FROM stdin;
1	Support
2	Teknisk Fråga
3	Fakturering
4	Allmänt
5	Retur
6	Reklamation
7	Leveransfråga
8	Produktinformation
9	Garanti
10	Uppdateringar
11	Installation
12	Avtal
13	Klagomål
14	Förslag
15	Övrigt
\.


--
-- Data for Name: company; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.company (id, company_name, company_phone, company_email) FROM stdin;
1	Företag A	010-1111111	info@foretaga.se
2	Företag B	010-2222222	info@foretagb.se
3	Företag C	010-3333333	info@foretagc.se
4	Företag D	010-4444444	info@foretagd.se
5	Företag E	010-5555555	info@foretage.se
6	Företag F	010-6666666	info@foretagf.se
7	Företag G	010-7777777	info@foretagg.se
8	Företag H	010-8888888	info@foretagh.se
9	Företag I	010-9999999	info@foretagi.se
10	Företag J	010-1010101	info@foretagj.se
11	Företag K	010-2020202	info@foretagk.se
12	Företag L	010-3030303	info@foretagl.se
13	Företag M	010-4040404	info@foretagm.se
14	Företag N	010-5050505	info@foretagn.se
15	Företag O	010-6060606	info@foretago.se
\.


--
-- Data for Name: employees; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.employees (id, user_id, company_id) FROM stdin;
1	2	1
5	12	5
6	14	6
10	9	10
7	5	1
13	3	13
12	13	12
11	11	11
4	8	4
\.


--
-- Data for Name: messages; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.messages (id, ticket_id, message, user_id) FROM stdin;
3	2	Hur uppdaterar jag produkten?	3
5	3	Jag förstår inte fakturan.	5
6	3	Låt oss gå igenom fakturan tillsammans.	7
9	5	Kan jag returnera produkten?	8
10	5	Självklart, här är returinstruktioner.	12
15	8	Kan jag få mer information om produkten?	13
\.


--
-- Data for Name: product; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.product (id, name, description, company_id) FROM stdin;
2	Produkt B1	Beskrivning av produkt B1	2
3	Produkt C1	Beskrivning av produkt C1	3
4	Produkt D1	Beskrivning av produkt D1	4
5	Produkt E1	Beskrivning av produkt E1	5
6	Produkt F1	Beskrivning av produkt F1	6
7	Produkt G1	Beskrivning av produkt G1	7
8	Produkt H1	Beskrivning av produkt H1	8
9	Produkt I1	Beskrivning av produkt I1	9
10	Produkt J1	Beskrivning av produkt J1	10
11	Produkt K1	Beskrivning av produkt K1	11
12	Produkt L1	Beskrivning av produkt L1	12
13	Produkt M1	Beskrivning av produkt M1	13
14	Produkt N1	Beskrivning av produkt N1	14
15	Produkt O1	Beskrivning av produkt O1	15
\.


--
-- Data for Name: ticket_tokens; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.ticket_tokens (id, ticket_id, token, expires_at) FROM stdin;
\.


--
-- Data for Name: tickets; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tickets (id, company_id, user_id, employee_id, product_id, category_id, date, title, description, status_id, case_number, token) FROM stdin;
2	2	3	4	2	1	2025-02-06 20:45:05.494515	Fråga om Produkt B1	Detaljer om frågan kring Produkt B1	3	2	\N
14	14	10	4	14	14	2025-02-06 20:45:05.494515	Förslag på förbättring	Kundens förslag	3	5	\N
8	8	13	4	8	8	2025-02-06 20:45:05.494515	Produktinformation för Produkt H1	Förfrågan om specifikationer	3	4	\N
15	15	11	7	15	15	2025-02-06 20:45:05.494515	Övriga frågor	Övriga frågor från kund	2	3	\N
3	3	5	7	3	3	2025-02-06 20:45:05.494515	Faktura för Produkt C1	Detaljer om fakturafrågan	2	6	\N
5	5	8	12	5	5	2025-02-06 20:45:05.494515	Retur av Produkt E1	Förfrågan om retur	5	7	\N
11	11	5	12	11	11	2025-02-06 20:45:05.494515	Installation av Produkt K1	Hjälp med installation	4	8	\N
\.


--
-- Data for Name: ticketstatus; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.ticketstatus (id, status_name) FROM stdin;
1	Unread
2	In Progress
3	Resolved
4	Closed
5	Pending
\.


--
-- Data for Name: userroles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.userroles (id, name) FROM stdin;
1	Customer
2	Employee
3	Admin
4	SuperAdmin
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, name, email, password, phonenumber, role_id) FROM stdin;
2	Bertil Berg	bertil@exempel.se	pass123	070-2222222	2
3	Cecilia Carlsson	cecilia@exempel.se	pass123	070-3333333	1
5	Erik Eriksson	erik@exempel.se	pass123	070-5555555	3
6	Frida Fransson	frida@exempel.se	pass123	070-6666666	1
8	Helena Holm	helena@exempel.se	pass123	070-8888888	1
9	Ivan Isaksson	ivan@exempel.se	pass123	070-9999999	2
10	Jenny Johansson	jenny@exempel.se	pass123	070-1010101	1
11	Karl Karlsson	karl@exempel.se	pass123	070-2020202	3
13	Martin Mattsson	martin@exempel.se	pass123	070-4040404	2
14	Nina Nilsson	nina@exempel.se	pass123	070-5050505	1
15	Oskar Olsson	oskar@exempel.se	pass123	070-6060606	2
22	SigmaMale	asdasdr444	8dda838f	\N	1
20	asdasd	asdasd	123krikkkk123	\N	1
21	asdasd	john@example.com	123krikkkk123	\N	1
18	John	John	123krikkkk123	\N	1
23	\N	SigmaMale3332424	87ce569c	\N	1
24	No Name	natna34tn	4962fbf5	\N	1
25	No Name	gna4nga4g	4ed095a0	\N	1
12	Linda Larsson	linda@exempel.se	pass123	070-3030303	4
7	Gustav Gustavsson	gustav@exempel.se	pass123	070-7777777	1
26	Test	test@exempel.se	5668c936	070-0101010	1
27	Test	Test@exempel.se	92111f53	070-10101010	1
\.


--
-- Name: category_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.category_id_seq', 15, true);


--
-- Name: company_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.company_id_seq', 15, true);


--
-- Name: employees_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.employees_id_seq', 17, true);


--
-- Name: messages_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.messages_id_seq', 22, true);


--
-- Name: product_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.product_id_seq', 18, true);


--
-- Name: ticket_tokens_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ticket_tokens_id_seq', 1, false);


--
-- Name: tickets_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.tickets_id_seq', 21, true);


--
-- Name: ticketstatus_column_name_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ticketstatus_column_name_seq', 5, true);


--
-- Name: userroles_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.userroles_id_seq', 4, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_id_seq', 27, true);


--
-- Name: category category_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.category
    ADD CONSTRAINT category_pkey PRIMARY KEY (id);


--
-- Name: company company_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_pkey PRIMARY KEY (id);


--
-- Name: employees employees_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_pkey PRIMARY KEY (id);


--
-- Name: messages messages_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_pkey PRIMARY KEY (id);


--
-- Name: product product_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_pkey PRIMARY KEY (id);


--
-- Name: ticket_tokens ticket_tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ticket_tokens
    ADD CONSTRAINT ticket_tokens_pkey PRIMARY KEY (id);


--
-- Name: ticket_tokens ticket_tokens_token_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ticket_tokens
    ADD CONSTRAINT ticket_tokens_token_key UNIQUE (token);


--
-- Name: tickets tickets_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_pkey PRIMARY KEY (id);


--
-- Name: ticketstatus ticketstatus_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ticketstatus
    ADD CONSTRAINT ticketstatus_pk PRIMARY KEY (id);


--
-- Name: userroles userroles_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.userroles
    ADD CONSTRAINT userroles_pkey PRIMARY KEY (id);


--
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: employees employees_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(id);


--
-- Name: employees employees_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: messages messages_ticket_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;


--
-- Name: messages messages_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.messages
    ADD CONSTRAINT messages_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: product product_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(id);


--
-- Name: ticket_tokens ticket_tokens_ticket_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ticket_tokens
    ADD CONSTRAINT ticket_tokens_ticket_id_fkey FOREIGN KEY (ticket_id) REFERENCES public.tickets(id) ON DELETE CASCADE;


--
-- Name: tickets tickets___fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets___fk FOREIGN KEY (status_id) REFERENCES public.ticketstatus(id);


--
-- Name: tickets tickets_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.category(id);


--
-- Name: tickets tickets_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(id);


--
-- Name: tickets tickets_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(id) ON DELETE CASCADE;


--
-- Name: tickets tickets_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.product(id);


--
-- Name: tickets tickets_users_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_users_id_fk FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: users users_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.userroles(id);


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE USAGE ON SCHEMA public FROM PUBLIC;


--
-- PostgreSQL database dump complete
--

