import { getInvestments } from "../data/get-investments";
import InvestmentsView from "./investments-view";

export default async function InvestmentsSection() {
  const portfolio = await getInvestments();
  return <InvestmentsView portfolio={portfolio} />;
}
