create table pulling (
	id text not null 
);

create table PedidoCompleto(
  id_db serial primary key,
  id text not null unique,
  displayId text,
  createdAt text,
  orderTiming text, 
  orderType text, 
  preparationStartDateTime text, 
  isTest bool, 
  salesChannel text
);

CREATE TABLE Delivery (
  id serial primary key,
  id_pedido text references pedidocompleto(id),
  mode text,
  deliveredBy text, 
  deliveryDateTime text, 
  observations text,
  pickupCode text
);

create table DeliveryAddress(
  id serial primary key, 
  id_delivery int references delivery(id),
  id_pedido text references pedidocompleto(id),
  streetName text, 
  streetNumber text,
  formattedAddress text, 
  neighborhood text, 
  complement text, 
  postalCode text, 
  city text, 
  reference text 
);

create table coordinates (
	id serial primary key, 
  id_DeliveryAddress int references deliveryaddress(id),
  id_pedido text references pedidocompleto(id),
  latitude float, 
  longitude float
);

create table merchant(
	id_pedido text references pedidocompleto(id),
	id text, 
  name text
);

create table customer(
  id text,
  id_pedido text references pedidocompleto(id),
  name text,
  documentNumber text,
  segmentation text
);

create table phone(
  id serial primary key,
  id_customer int references customer(id),
  id_pedido text references pedidocompleto(id),
  number text,
  localizer text,
  localizerExpiration text
);



create table Items(
  item_id serial primary key, 
  id_pedido text references pedidocompleto(id),
  index int, 
  id text, 
  uniqueId text,
  name text, 
  quantity int,
  unit text, 
  unitPrice float, 
  optionsPrice float, 
  totalPrice float, 
  price float 
);

create table total(
  id serial primary key,
  id_pedido text references pedidocompleto(id),
  additionalFees float, 
  subTotal float, 
  deliveryFee float,
  benefits int,
  orderAmount float
);

create table Payments(
  id serial primary key,
  id_pedido text references pedidocompleto(id),
  prepaid float,
  pending int
);

create table Methods(
  id serial primary key,
  payments_id int references payments(id),
  id_pedido text references pedidocompleto(id),
  value float,
  currency text,
  method text,
  prepaid bool, 
  type text
);

create table card(
	id serial primary key, 
  methods_id int references methods(id),
  id_pedido text references pedidocompleto(id),
  brand text
);

create table AdditionalInfo(
  id serial primary key,
  id_pedido text references pedidocompleto(id)
);

create table metadata(
  id serial primary key, 
  id_additionalInfo int references additionalinfo(id),
  id_pedido text references pedidocompleto(id),
  developerId text,
  customerEmail text,
  developerEmail text
);



delete from coordinates;
delete from deliveryaddress;
delete from delivery;
delete from merchant;
delete from phone;
delete from customer;
delete from methods;
delete from payments;
delete from items;
delete from total;
delete from metadata;
delete from additionalinfo;
delete from pedidocompleto;

                                  