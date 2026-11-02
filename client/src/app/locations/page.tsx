import type { Metadata } from "next";
import { sections } from "@/shared/config/routes";
import { LocationsList } from "@/widgets/locations-list";

export const metadata: Metadata = {
  title: sections.locations.label + " | Directory Service",
};

export default function LocationsPage() {
  return <LocationsList />;
}
